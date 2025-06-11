using System.Collections.Generic;

// Структуры данных, используемые для логирования игровых сессий
// Используются GameManager и LogManager

public struct StartGame
{
  public int BaseSeed;
  public string NameGame;
  public string VersionGame;
  public int StartLevel;
  public float TimeSession;
}

public struct LevelData
{
  public int Level;
  public int StreakCorrectQuestionsToLevelUp;
  public int CountMistakesToLevelDown;
  public int CountAnswersToQuestion;
  public int CountMistakesForQuestion;
  public float TimeToMemorize;
  public float TimeToForget;
  public float TimeQuestionLimit;
  public float ResultDisplayTime;
  public Dictionary<string, object> SpecificData;

  public LevelData(
    int level, 
    int streakCorrectQuestionsToLevelUp, 
    int countMistakesToLevelDown, 
    int countAnswersToQuestion, 
    int countMistakesForQuestion, 
    float timeToMemorize, 
    float timeToForget, 
    float timeQuestionLimit, 
    float resultDisplayTime, 
    Dictionary<string, object> specificData)
  {
    Level = level;
    StreakCorrectQuestionsToLevelUp = streakCorrectQuestionsToLevelUp;
    CountMistakesToLevelDown = countMistakesToLevelDown;
    CountAnswersToQuestion = countAnswersToQuestion;
    CountMistakesForQuestion = countMistakesForQuestion;
    TimeToMemorize = timeToMemorize;
    TimeToForget = timeToForget;
    TimeQuestionLimit = timeQuestionLimit;
    ResultDisplayTime = resultDisplayTime;
    SpecificData = specificData;
  }
}

public struct LevelResults
{
  public StatusLevelList StatusLevel;
  public float ReactionTime;
}

public struct QuestionData
{
  /// <summary> Указывает игровой менеджер </summary>
  public int QuestionNumber;

  /// <summary> Указывает игровой скрипт </summary>
  public string QuestionText;

  /// <summary> Указывает игровой скрипт </summary>
  public string ExpectedAnswer;

  /// <summary> Указывает игровой скрипт </summary>
  public Dictionary<string, object> SpecificData;

  public QuestionData(string questionText, string expectedAnswer, Dictionary<string, object> specificData)
  {
    QuestionText = questionText;
    ExpectedAnswer = expectedAnswer;
    SpecificData = specificData;

    QuestionNumber = -1;
  }
}

  public struct AnswerData
{
  /// <summary> Указывает игровой менеджер </summary>
  public int QuestionNumber;

  /// <summary> Указывает игровой скрипт </summary>
  public string AnswerText;

  /// <summary> Указывает игровой скрипт </summary>
  public bool IsCorrect;

  /// <summary> Указывает игровой менеджер </summary>
  public float ReactionTime;

  public AnswerData(string answerText, bool isCorrect)
  {
    AnswerText = answerText;
    IsCorrect = isCorrect;

    QuestionNumber = -1;
    ReactionTime = -1f;
  }
}
