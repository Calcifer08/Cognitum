using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;
  [SerializeField] private LogSessionManager _logSessionManager;
  [SerializeField] private ProgressManager _progressManager;
  [SerializeField] private TimerManager _timerManager;


  private string _nameGame;

  private int _countGame; // уст из файла при старте // чтоб генерить сиды для уровней
  private int _level = 1; // уст из файла при старте
  private int _maxLevel; // уст в игр скрип
  //private int _streakCorrectQuestionsToLevelUp; // уст в игр скрип // число правильных вопросов подряд, чтобы повысить лвл
  private int _correctQuestions = 0;
  //private int _correctAnswersToQuestion;
  [SerializeField] private int _correctAnswers = 0;
  //private int _сountMistakesToLevelDown; // число неправильных ответов подряд, чтобы снизить лвл
  private int _mistakes = 0;
  private int _countMistakesForQuestion = 0;
  private int _numberQuestion = 0;

  private List<float> _reactionTimes = new List<float>();
  private float _startPhaseAnswer; // время конца последнего вопроса или ответа (для расчёта времени реакции)

  private void Awake()
  {
    // отписки не нужны, т.к. все объекты живут одинаково
    _timerManager.OnGameTimeOver += SaveSession;
    _timerManager.OnPauseStarted += _logSessionManager.AddPauseStartToLog;
    _timerManager.OnPauseEnded += _logSessionManager.AddPauseEndToLog;
    _timerManager.OnEndTimeAnswerPhase += () => StartCoroutine(IncorrectAnswerCoroutine(true));

    _gameBuilder.OnSetAnswer += GetAnswer;
  }

  private void Start()
  {
    _nameGame = _gameBuilder.NameGame; // для удобства
    GameConfig gameConfig = GameConfigManager.GetGameConfig(_nameGame);
    _level = gameConfig.CurrentLevel;
    _countGame = gameConfig.CountGame;

    GameResultsManager.InitResults(_level);

    _maxLevel = _gameBuilder.MaxLevel;

    _gameBuilder.GetBaseSeedForGame(_countGame);

    StartGame startGame = new StartGame()
    {
      BaseSeed = _gameBuilder.BaseSeed,
      NameGame = _nameGame,
      VersionGame = _gameBuilder.VersionGame,
      StartLevel = _level,
      TimeSession = _gameBuilder.TimeSession
    };

    _timerManager.SetTimerSession(_gameBuilder.TimeSession);

    _logSessionManager.StartGameSessionLog(startGame);

    // передаём такой статус, просто потому что не хочется пока делать флаг начала игры,
    // ну и это значение не вызывается нигде, кроме конца игры
    UpdateLevel(StatusLevelList._);
  }

  public void GetAnswer(AnswerData answerData)
  {
    UpdateAnswer(answerData);

    if (answerData.IsCorrect)
    {
      StartCoroutine(CorrectAnswerCoroutine());
    }
    else
    {
      StartCoroutine(IncorrectAnswerCoroutine(false));
    }
  }

  private IEnumerator CorrectAnswerCoroutine()
  {
    SoundManager.Instance.PlaySoundAnswer(true);

    _progressManager.AddPointsForCorrectAnswer();
    _correctAnswers++;
    //_mistakes = 0; // сбрасываем ошибки
    GameResultsManager.AddCorrectAnswer();
    UpdateScore(); // убрать, если не будет ответов в ui

    // если дали все ответы на вопрос
    if (_correctAnswers >= _gameBuilder.CountAnswersToQuestion)
    {
      _timerManager.CancelAnswerTimer(); // если таймер был запущен
      _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.Win);
      _correctQuestions++;
      UpdateScore();

      // если ответили на все вопросы и можем повысить лвл
      if (_correctQuestions >= _gameBuilder.StreakCorrectQuestionsToLevelUp)
      {
        if (_level < _maxLevel)
        {
          _level++;
        }

        GameResultsManager.AddWinLevel(_level);
        _progressManager.AddPointsForLevelWin();
        yield return StartCoroutine(UpdateGameZoneStateCoroutine(true));
        UpdateLevel(StatusLevelList.Win);
      }
      else
      {
        yield return StartCoroutine(UpdateGameZoneStateCoroutine(true));
        UpdateQuestion();
      }
    }
    else
    {
      yield return StartCoroutine(_progressManager.DisplayResultCoroutine(true, _gameBuilder.ResultDisplayTime));
    }
  }

  private IEnumerator IncorrectAnswerCoroutine(bool isTimeOut)
  {
    SoundManager.Instance.PlaySoundAnswer(false);
    _progressManager.SubtractPointsForIncorrectAnswer();
    _mistakes++;
    _countMistakesForQuestion++;
    _correctQuestions = 0; //сбрасываем правильные вопросы

    // т.к. это не ошибочный ответ, а просто его отсутствие
    if (isTimeOut)
    {
      GameResultsManager.AddSkippedAnswer();
      _gameBuilder.TimeAnswerDone();
    }  
    else
      GameResultsManager.AddIncorrectAnswer();

    UpdateScore();

    bool isQuestionFailed = _countMistakesForQuestion >= _gameBuilder.CountMistakesForQuestion || isTimeOut;
    // если достигли макса неправильных ответов на вопрос
    if (isQuestionFailed)
    {
      if (isTimeOut)
      {
        _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.TimeDone);
      }
      else
      {
        _timerManager.CancelAnswerTimer(); // если таймер был запущен
        _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.Fail);
      }
    }

    // если достигли макса неправильных ответов на весь уровень
    if (_mistakes >= _gameBuilder.CountMistakesToLevelDown)
    {
      if (_level > 1)
      {
        _level--;
      }

      GameResultsManager.AddFailLevel(_level);
      _progressManager.SubtractPointsForLevelFailure();
      yield return StartCoroutine(UpdateGameZoneStateCoroutine(false));
      UpdateLevel(StatusLevelList.Fail);
    }
    else
    {
      // если надо обновить вопрос
      if (isQuestionFailed)
      {
        yield return StartCoroutine(UpdateGameZoneStateCoroutine(false));
        UpdateQuestion();
      }
      else
      {
        yield return StartCoroutine(_progressManager.DisplayResultCoroutine(false, _gameBuilder.ResultDisplayTime));
      }
    }
  }

  private IEnumerator UpdateGameZoneStateCoroutine(bool isCorrectAnswer)
  {
    _gameBuilder.SetInteractableGameZone(false);
    yield return StartCoroutine(_progressManager.DisplayResultCoroutine(isCorrectAnswer, _gameBuilder.ResultDisplayTime));
    _gameBuilder.SetInteractableGameZone(true);
  }

  private void UpdateLevel(StatusLevelList status)
  {
    _numberQuestion = 0; // обнуляем для новго уровня
    _correctQuestions = 0; // сбрасываем для нового уровня/раунда
    //_correctAnswers = 0; // всё равно сбросим в вопросе
    _mistakes = 0;

    //  ибо StatusLevelList._ просто заглушка для старта
    if (status != StatusLevelList._)
    {
      LevelResults levelResults = new LevelResults()
      {
        StatusLevel = status,
        ReactionTime = CalculateReactionTime()
      };

      _logSessionManager.LevelResultsLog(levelResults);
    }

    //SetDataFromGame();
    _gameBuilder.GameSetLevel(_level);

    _progressManager.UpdateLevel(_level);
    _progressManager.UpdatePointsForLevel(_level); // обновляем очки под текущий уровень
    UpdateScore();

    LevelData levelData = new LevelData(
      _level,
      _gameBuilder.StreakCorrectQuestionsToLevelUp,
      _gameBuilder.CountMistakesToLevelDown,
      _gameBuilder.CountAnswersToQuestion,
      _gameBuilder.CountMistakesForQuestion,
      _gameBuilder.TimeMemorizePhase,
      _gameBuilder.TimeForgetPhase,
      _gameBuilder.TimeAnswerPhase,
      _gameBuilder.ResultDisplayTime,
      _gameBuilder.GetSpecificData(true)
    );

    _logSessionManager.AddLevelToLog(levelData);

    UpdateQuestion();
  }

  private void UpdateScore()
  {
    _progressManager.UpdateCorrect(_correctQuestions, _gameBuilder.StreakCorrectQuestionsToLevelUp);
    _progressManager.UpdateMistake(_mistakes, _gameBuilder.CountMistakesToLevelDown);
  }

  // должен добавить вопрос из игрового скрипта, если вопроса нет, то ничего не делает
  private void UpdateQuestion()
  {
    if (_numberQuestion != 0)
    {
      _logSessionManager.AddQuestionEndToLog(_numberQuestion);
    }

    _numberQuestion++;
    _correctAnswers = 0;
    _countMistakesForQuestion = 0;

    _gameBuilder.GameUpdateQuestion();

    QuestionData questionData = new QuestionData
    {
      QuestionNumber = _numberQuestion,
      QuestionText = _gameBuilder.TextQuestion,
      ExpectedAnswer = _gameBuilder.TextAnswer,
      SpecificData = _gameBuilder.GetSpecificData(false),
    };

    _logSessionManager.AddQuestionStartToLog(questionData);

    StartCoroutine(RunGamePhasesCoroutine());
  }

  private IEnumerator RunGamePhasesCoroutine()
  {
    // если есть фаза запоминания
    if (_gameBuilder.TimeMemorizePhase > 0f)
    {
      _logSessionManager.AddMemorizePhaseStartToLog(_numberQuestion, _gameBuilder.TimeMemorizePhase);
      yield return StartCoroutine(_gameBuilder.StartMemorizePhaseCoroutine());
      _logSessionManager.AddMemorizePhaseEndToLog(_numberQuestion);
    }

    // если есть фаза забывания
    if (_gameBuilder.TimeForgetPhase > 0f)
    {
      _logSessionManager.AddForgetPhaseStartToLog(_numberQuestion, _gameBuilder.TimeForgetPhase);
      yield return StartCoroutine(_gameBuilder.StartForgetPhaseCoroutine());
      _logSessionManager.AddForgetPhaseEndToLog(_numberQuestion);
    }

    // если есть время на ответы
    if (_gameBuilder.TimeAnswerPhase > 0f)
    {
      _timerManager.SetTimerQuestion(_gameBuilder.TimeAnswerPhase);
    }

    _startPhaseAnswer = Time.time;
    _logSessionManager.AddAnswerPhaseStartToLog(_numberQuestion, _gameBuilder.TimeAnswerPhase);
    _gameBuilder.SetInteractableGameZone(true);
  }

  private void UpdateAnswer(AnswerData answerData)
  {
    answerData.QuestionNumber = _numberQuestion;
    answerData.ReactionTime = Time.time - _startPhaseAnswer;

    _startPhaseAnswer = Time.time; 

    _reactionTimes.Add(answerData.ReactionTime);

    //LogLevelEvents events = new LogLevelEvents(TagsList.AnswerSubmitted, GetTimeString(), answerData);

    //_logSessionManager.AddEventToCurrentLevel(events);
    _logSessionManager.AddAnswerToLog(answerData);
  }

  public void SaveSession()
  {
    _countGame++; // повышаем число сыгранных игр

    LevelResults levelResults = new LevelResults()
    {
      StatusLevel = StatusLevelList.TimeDone,
      ReactionTime = CalculateReactionTime()
    };

    _logSessionManager.LevelResultsLog(levelResults);

    _logSessionManager.EndGameSessionLog(); // сохраняем лог

    // сделал синхронным, ибо не много данных
    _ = GameSessionManager.SaveSessionDataAsync(_nameGame, _progressManager.Score, _level); // перенести очки из score

    GameResultsManager.SetScore(_progressManager.Score);
    GameResultsManager.SetMaxScore(GameConfigManager.GetGameConfig(_nameGame).MaxScore);

    // мне не надо ждать его заверешения на этой сцене, так что можно запустить без ожидания заверешения
    // _ = просто заглушка предупрежедния
    _ = GameConfigManager.UpdateConfigForGameAsync(_nameGame, _level, _countGame, _progressManager.Score);

    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneNames.EndGame);
  }

  // раньше возвращал nan, если было пусто, заменил на -1, ибо в монгоДБ не принимается nan в число
  private float CalculateReactionTime()
  {
    if (_reactionTimes.Count == 0)
    {
      return -1f;
    }

    float allTime = 0f;
    foreach (var times in _reactionTimes)
    {
      allTime += times;
    }

    float reaction = allTime / _reactionTimes.Count;
    _reactionTimes.Clear();
    return reaction;
  }
}
