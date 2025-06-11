using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LogSessionManager : MonoBehaviour
{
  public const string Folder = "Logs";
  private LogGame _logGame;
  private LogLevelData currentLevel;

  private static string _logsDirectoryPath;

  private Stopwatch _sessionStopwatch;

  public void StartGameSessionLog(StartGame startGame)
  {
    _sessionStopwatch = new Stopwatch();
    _sessionStopwatch.Start();

    string startTime = DateTime.Now.ToString("O");
    string timeZone = TimeZoneInfo.Local.ToString();

    PlayerData playerData = PlayerDataManager.GetPlayerData();
    DeviceMetadata deviceMetadata = new DeviceMetadata(Screen.width, Screen.height, SystemInfo.operatingSystem);
    LogGameInfo gameInfo = new LogGameInfo(startGame.NameGame, startGame.VersionGame);
    LogSessionInfo sessionInfo = new LogSessionInfo(
      new LogSessionParams(startGame.BaseSeed, startGame.TimeSession), timeZone, startTime, startGame.StartLevel);

    _logGame = new LogGame(playerData, deviceMetadata, gameInfo, sessionInfo);

    CreateLogsDirectory();

    Debug.Log("Начата запись лога игры");
  }

  public static void CreateLogsDirectory()
  {
    _logsDirectoryPath = Path.Combine(Application.persistentDataPath, Folder);

    if (!Directory.Exists(_logsDirectoryPath))
    {
      Directory.CreateDirectory(_logsDirectoryPath);
      Debug.Log("Папка логов создана");
    }
  }

  public void AddLevelToLog(LevelData levelData)
  {
    if (!LogLevelData.ValidateSpecificData(levelData.SpecificData))
    {
      Debug.LogWarning("Обнаружены неподдерживаемые типы в специфичных данных.");
      return;
    }

    LogLevelParams logLevelParams = new LogLevelParams(
      levelData.Level,
      levelData.StreakCorrectQuestionsToLevelUp,
      levelData.CountMistakesToLevelDown,
      levelData.CountAnswersToQuestion,
      levelData.CountMistakesForQuestion,
      levelData.TimeToMemorize,
      levelData.TimeToForget,
      levelData.TimeQuestionLimit,
      levelData.ResultDisplayTime,
      levelData.SpecificData);

    LogLevelData logLevel = new LogLevelData(
      logLevelParams,
      GetTimeString());

    _logGame.Levels.Add(logLevel);
    currentLevel = logLevel;

    Debug.Log("Уровень добавлен");
  }

  public void LevelResultsLog(LevelResults levelResultsFromGame)
  {
    if (currentLevel == null)
    {
      Debug.LogError("Нет уровня для завершения");
      return;
    }

    var levelResultsFromLog = currentLevel.LevelResults;
    levelResultsFromLog.TimeEnd = GetTimeString();
    TimeSpan start = TimeSpan.Parse(levelResultsFromLog.TimeStart);
    TimeSpan end = TimeSpan.Parse(levelResultsFromLog.TimeEnd);
    levelResultsFromLog.CompletionTime = (float)(end - start).TotalSeconds;
    levelResultsFromLog.ReactionTime = levelResultsFromGame.ReactionTime;
    levelResultsFromLog.Status = levelResultsFromGame.StatusLevel;

    var sessionResults = _logGame.Session.SessionResults;
    sessionResults.LevelsCompleted++;
    sessionResults.MaxLevelReached = Math.Max(sessionResults.MaxLevelReached, currentLevel.LevelParams.Level);
    sessionResults.CorrectQuestions += levelResultsFromLog.CorrectQuestions;
    sessionResults.MistakeQuestions += levelResultsFromLog.MistakeQuestions;
    sessionResults.CorrectAnswers += levelResultsFromLog.CorrectAnswers;
    sessionResults.MistakeAnswers += levelResultsFromLog.MistakeAnswers;
    sessionResults.SkippedAnswers += levelResultsFromLog.SkippedAnswers;

    if (levelResultsFromGame.StatusLevel == StatusLevelList.Win)
    {
      sessionResults.MaxLevelCompleted = Math.Max(sessionResults.MaxLevelCompleted, currentLevel.LevelParams.Level);
    }

    Debug.Log("Уровень завершён");
  }

  public async void EndGameSessionLog()
  {
    if (transform.parent != null)
    {
      transform.SetParent(null);
    }

    DontDestroyOnLoad(gameObject);

    var sessionResults = _logGame.Session.SessionResults;
    sessionResults.TimeEnd = DateTime.Now.ToString("O");
    sessionResults.FinalLevel = _logGame.Levels.Count > 0 ? currentLevel.LevelParams.Level : 1;

    _sessionStopwatch.Stop();

    string dateFile = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    string fileName = $"LogSession_{_logGame.Player.UserID}_{dateFile}.json";
    string filePath = Path.Combine(_logsDirectoryPath, fileName);

    await SaveLogAsync(filePath);
  }

  private async Task SaveLogAsync(string path)
  {
#if UNITY_ANDROID && !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_logGame, Formatting.None);
#elif UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_logGame, Formatting.Indented);
#endif

    try
    {
      await File.WriteAllTextAsync(path, json);
      Debug.Log("Лог сохранён");
    }
    catch (Exception ex)
    {
      Debug.LogError($"Ошибка при сохранении лога: {ex.Message}");
    }
    finally
    {
      Destroy(gameObject);
    }
  }

  private string GetTimeString()
  {
    TimeSpan ts = _sessionStopwatch.Elapsed;
    return ts.ToString("c");
  }



  private void AddEventToCurrentLevel(LogLevelEvents logEvent)
  {
    if (currentLevel == null)
    {
      Debug.LogError("Нельзя добавить событие, нет текущего уровня");
      return;
    }

    currentLevel.LevelEvents.Add(logEvent);
    Debug.Log("Событие добавлено в текущий уровень");
  }

  public void AddQuestionStartToLog(QuestionData questionData)
  {
    if (!LogLevelData.ValidateSpecificData(questionData.SpecificData))
    {
      Debug.LogWarning("Обнаружены неподдерживаемые типы в специфичных данных.");
      return;
    }

    EventQuestionStart questionStart = new EventQuestionStart(
        questionData.QuestionNumber,
        questionData.QuestionText,
        questionData.ExpectedAnswer,
        questionData.SpecificData
    );

    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.QuestionStart, GetTimeString(), questionStart);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddQuestionEndToLog(int questionNumber)
  {
    EventQuestionEnd questionEnd = new EventQuestionEnd(questionNumber);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.QuestionEnd, GetTimeString(), questionEnd);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddAnswerToLog(AnswerData answerData)
  {
    EventAnswer eventAnswer = new EventAnswer(
      answerData.QuestionNumber,
      answerData.AnswerText,
      answerData.IsCorrect,
      answerData.ReactionTime);

    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.AnswerSubmitted, GetTimeString(), eventAnswer);
    AddEventToCurrentLevel(levelEvent);

    if (answerData.IsCorrect)
    {
      currentLevel.LevelResults.CorrectAnswers++;
    }
    else
    {
      currentLevel.LevelResults.MistakeAnswers++;
    }
  }


  public void AddMemorizePhaseStartToLog(int questionNumber, float duration)
  {
    EventMemorizePhaseStart memorizePhaseStart = new EventMemorizePhaseStart(questionNumber, duration);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.MemorizePhaseStart, GetTimeString(), memorizePhaseStart);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddMemorizePhaseEndToLog(int questionNumber)
  {
    EventMemorizePhaseEnd memorizePhaseEnd = new EventMemorizePhaseEnd(questionNumber);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.MemorizePhaseEnd, GetTimeString(), memorizePhaseEnd);
    AddEventToCurrentLevel(levelEvent);
  }


  public void AddForgetPhaseStartToLog(int questionNumber, float duration)
  {
    EventForgetPhaseStart forgetPhaseStart = new EventForgetPhaseStart(questionNumber, duration);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.ForgetPhaseStart, GetTimeString(), forgetPhaseStart);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddForgetPhaseEndToLog(int questionNumber)
  {
    EventForgetPhaseEnd forgetPhaseEnd = new EventForgetPhaseEnd(questionNumber);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.ForgetPhaseEnd, GetTimeString(), forgetPhaseEnd);
    AddEventToCurrentLevel(levelEvent);
  }


  public void AddAnswerPhaseStartToLog(int questionNumber, float duration)
  {
    EventAnswerPhaseStart answerPhaseStart = new EventAnswerPhaseStart(questionNumber, duration);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.AnswerPhaseStart, GetTimeString(), answerPhaseStart);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddAnswerPhaseEndToLog(int questionNumber, StatusQuestionList status)
  {
    EventAnswerPhaseEnd answerPhaseEnd = new EventAnswerPhaseEnd(questionNumber, status);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.AnswerPhaseEnd, GetTimeString(), answerPhaseEnd);
    AddEventToCurrentLevel(levelEvent);

    if (status == StatusQuestionList.Win)
      currentLevel.LevelResults.CorrectQuestions++;
    else if (status == StatusQuestionList.TimeDone)
      currentLevel.LevelResults.SkippedAnswers++;
    else
      currentLevel.LevelResults.MistakeQuestions++;
  }


  public void AddPauseStartToLog(int pauseNumber, string description)
  {
    EventPauseStart eventPauseStart = new EventPauseStart(pauseNumber, description);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.PauseStart, GetTimeString(), eventPauseStart);
    AddEventToCurrentLevel(levelEvent);
  }

  public void AddPauseEndToLog(int pauseNumber, float duration)
  {
    EventPauseEnd eventPauseEnd = new EventPauseEnd(pauseNumber, duration);
    LogLevelEvents levelEvent = new LogLevelEvents(TagsList.PauseEnd, GetTimeString(), eventPauseEnd);
    AddEventToCurrentLevel(levelEvent);
  }
}
