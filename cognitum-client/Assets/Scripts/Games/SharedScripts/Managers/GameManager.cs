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

  private int _countGame;
  private int _level = 1;
  private int _maxLevel;
  private int _correctQuestions = 0;
  [SerializeField] private int _correctAnswers = 0;
  private int _mistakes = 0;
  private int _countMistakesForQuestion = 0;
  private int _numberQuestion = 0;

  private List<float> _reactionTimes = new List<float>();
  private float _startPhaseAnswer;

  private void Awake()
  {
    _timerManager.OnGameTimeOver += SaveSession;
    _timerManager.OnPauseStarted += _logSessionManager.AddPauseStartToLog;
    _timerManager.OnPauseEnded += _logSessionManager.AddPauseEndToLog;
    _timerManager.OnEndTimeAnswerPhase += () => StartCoroutine(IncorrectAnswerCoroutine(true));

    _gameBuilder.OnSetAnswer += GetAnswer;
  }

  private void Start()
  {
    _nameGame = _gameBuilder.NameGame;
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
    GameResultsManager.AddCorrectAnswer();
    UpdateScore();

    if (_correctAnswers >= _gameBuilder.CountAnswersToQuestion)
    {
      _timerManager.CancelAnswerTimer();
      _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.Win);
      _correctQuestions++;
      UpdateScore();

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
    _correctQuestions = 0;

    if (isTimeOut)
    {
      GameResultsManager.AddSkippedAnswer();
      _gameBuilder.TimeAnswerDone();
    }  
    else
      GameResultsManager.AddIncorrectAnswer();

    UpdateScore();

    bool isQuestionFailed = _countMistakesForQuestion >= _gameBuilder.CountMistakesForQuestion || isTimeOut;
    if (isQuestionFailed)
    {
      if (isTimeOut)
      {
        _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.TimeDone);
      }
      else
      {
        _timerManager.CancelAnswerTimer();
        _logSessionManager.AddAnswerPhaseEndToLog(_numberQuestion, StatusQuestionList.Fail);
      }
    }

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
    _numberQuestion = 0;
    _correctQuestions = 0;
    _mistakes = 0;

    if (status != StatusLevelList._)
    {
      LevelResults levelResults = new LevelResults()
      {
        StatusLevel = status,
        ReactionTime = CalculateReactionTime()
      };

      _logSessionManager.LevelResultsLog(levelResults);
    }

    _gameBuilder.GameSetLevel(_level);

    _progressManager.UpdateLevel(_level);
    _progressManager.UpdatePointsForLevel(_level);
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
    if (_gameBuilder.TimeMemorizePhase > 0f)
    {
      _logSessionManager.AddMemorizePhaseStartToLog(_numberQuestion, _gameBuilder.TimeMemorizePhase);
      yield return StartCoroutine(_gameBuilder.StartMemorizePhaseCoroutine());
      _logSessionManager.AddMemorizePhaseEndToLog(_numberQuestion);
    }

    if (_gameBuilder.TimeForgetPhase > 0f)
    {
      _logSessionManager.AddForgetPhaseStartToLog(_numberQuestion, _gameBuilder.TimeForgetPhase);
      yield return StartCoroutine(_gameBuilder.StartForgetPhaseCoroutine());
      _logSessionManager.AddForgetPhaseEndToLog(_numberQuestion);
    }

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

    _logSessionManager.AddAnswerToLog(answerData);
  }

  public void SaveSession()
  {
    _countGame++;

    LevelResults levelResults = new LevelResults()
    {
      StatusLevel = StatusLevelList.TimeDone,
      ReactionTime = CalculateReactionTime()
    };

    _logSessionManager.LevelResultsLog(levelResults);

    _logSessionManager.EndGameSessionLog();

    _ = GameSessionManager.SaveSessionDataAsync(_nameGame, _progressManager.Score, _level);

    GameResultsManager.SetScore(_progressManager.Score);
    GameResultsManager.SetMaxScore(GameConfigManager.GetGameConfig(_nameGame).MaxScore);

    _ = GameConfigManager.UpdateConfigForGameAsync(_nameGame, _level, _countGame, _progressManager.Score);

    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneNames.EndGame);
  }

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
