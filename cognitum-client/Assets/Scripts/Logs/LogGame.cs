using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

// весь файл
[System.Serializable]
public class LogGame
{
  public PlayerData Player;
  public DeviceMetadata Device;
  public LogGameInfo Game;
  public LogSessionInfo Session;
  public List<LogLevelData> Levels;

  public LogGame(PlayerData player, DeviceMetadata deviceMetadata, LogGameInfo game, LogSessionInfo sessionInfo)
  {
    Player = player;
    Device = deviceMetadata;
    Game = game;
    Session = sessionInfo;
    Levels = new List<LogLevelData>();
  }
}

// инфа по устройству
[System.Serializable]
public class DeviceMetadata
{
  public int DeviceScreenWidth;
  public int DeviceScreenHeight;
  public string DeviceOS;

  public DeviceMetadata(int deviceScreenWidth, int deviceScreenHeight, string deviceOS)
  {
    DeviceScreenWidth = deviceScreenWidth;
    DeviceScreenHeight = deviceScreenHeight;
    DeviceOS = deviceOS;
  }
}

// инфа по конкретной игре
[System.Serializable]
public class LogGameInfo
{ 
  public string GameName;
  public string GameVersion;

  public LogGameInfo (string gameName, string gameVersion)
  {
    GameName = gameName;
    GameVersion = gameVersion;
  }
}

// инфа по игровой сессии, хранит в себе настройки сессии и результаты
[System.Serializable]
public class LogSessionInfo
{
  public LogSessionParams SessionParams;
  public LogSessionResults SessionResults;

  public LogSessionInfo(LogSessionParams sessionParams, string timeZone, string datetime, int startLevel)
  {
    SessionParams = sessionParams;
    SessionResults = new LogSessionResults(timeZone, datetime, startLevel);
  }
}

// настройки сессии
[System.Serializable]
public class LogSessionParams
{
  public float AllowedTime; // время на прохождение сессии
  public int BaseSeed; // базовое зерно для построения игры

  public LogSessionParams(int baseSeed, float allowedTime)
  {
    BaseSeed = baseSeed;
    AllowedTime = allowedTime;
  }
}

// результаты сессии
[System.Serializable]
public class LogSessionResults
{
  public string TimeZone;
  public string TimeStart;
  public string TimeEnd;
  public int StartLevel;
  public int FinalLevel;
  public int MaxLevelReached; // макс до которого дошли (но не обязательно прошли)
  public int MaxLevelCompleted; // макс лвл, который игрок смог пройти
  public int LevelsCompleted = 0; // сколько завершили (с учётом поражений и истечении времени)
  public int CorrectQuestions = 0;
  public int MistakeQuestions = 0;
  public int CorrectAnswers = 0;
  public int MistakeAnswers = 0;
  public int SkippedAnswers = 0;

  public LogSessionResults(string timeZone, string datetime, int startLevel)
  {
    TimeZone = timeZone;
    TimeStart = datetime;
    StartLevel = startLevel;
    FinalLevel = startLevel;
    MaxLevelReached = startLevel;
    MaxLevelCompleted = startLevel;
  }
}

// данные по уровню
[System.Serializable]
public class LogLevelData
{
  public LogLevelParams LevelParams;
  public LogLevelResults LevelResults;
  public List<LogLevelEvents> LevelEvents;

  public LogLevelData(LogLevelParams logLevelParams, string time)
  {
    LevelParams = logLevelParams;
    LevelResults = new LogLevelResults(time);
    LevelEvents = new List<LogLevelEvents>();
  }

  public static bool ValidateSpecificData(Dictionary<string, object> data)
  {
    bool allValid = true;

    foreach (var pair in data)
    {
      object value = pair.Value;

      if (!(value is int || value is float || value is bool || value is string))
      {
        Debug.LogWarning($"[LogValidator] Ключ '{pair.Key}' содержит неподдерживаемый тип: {value?.GetType().Name ?? "null"}");
        allValid = false;
      }
    }

    return allValid;
  }
}

// настройки уровня
[System.Serializable]
public class LogLevelParams
{
  public int Level;
  public int StreakCorrectQuestionsToLevelUp;
  public int CountMistakesToLevelDown;
  public int CountAnswersToQuestion;
  public int CountMistakesForQuestion;
  public float TimeMemorizePhase;
  public float TimeForgetPhase;
  public float TimeAnswerPhase;
  public float ResultDisplayTime;
  public Dictionary<string, object> SpecificData;

  public LogLevelParams(
    int level, 
    int streakCorrectQuestionsToLevelUp, 
    int countMistakesToLevelDown,
    int countAnswersToQuestion,
    int countMistakesForQuestion,
    float timeMemorizePhase,
    float timeForgetPhase,
    float timeAnswerPhase,
    float resultDisplayTime,
    Dictionary<string, object> specificData)
  {
    Level = level;
    StreakCorrectQuestionsToLevelUp = streakCorrectQuestionsToLevelUp;
    CountMistakesToLevelDown = countMistakesToLevelDown;
    CountAnswersToQuestion = countAnswersToQuestion;
    CountMistakesForQuestion = countMistakesForQuestion;
    TimeMemorizePhase = timeMemorizePhase;
    TimeForgetPhase = timeForgetPhase;
    TimeAnswerPhase = timeAnswerPhase;
    ResultDisplayTime = resultDisplayTime;
    SpecificData = specificData;
  }
}

// результаты уровня (поднят наверх?)
[System.Serializable]
public class LogLevelResults
{
  public string TimeStart;
  public string TimeEnd;
  public int CorrectQuestions;
  public int MistakeQuestions;
  public int CorrectAnswers;
  public int MistakeAnswers;
  public int SkippedAnswers;
  public float CompletionTime;
  public float ReactionTime;

  [JsonConverter(typeof(StringEnumConverter))] // JsonConverter для конвертации Enum в строку, иначе будут инты
  public StatusLevelList Status;

  public LogLevelResults(string time)
  {
    TimeStart = time;
  }
}
