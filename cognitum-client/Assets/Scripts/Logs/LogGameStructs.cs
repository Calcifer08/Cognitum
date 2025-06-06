using System.Collections.Generic;

// GameLogStructs.cs
// —труктуры данных, используемые дл€ логировани€ игровых сессий
// »спользуютс€ GameManager и LogManager

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

// при добавлении большого числа полей от контроллера - стоит разбить на 2 сущности
public struct QuestionData
{
  /// <summary> ”казывает игровой менеджер </summary>
  public int QuestionNumber;

  /// <summary> ”казывает игровой скрипт </summary>
  public string QuestionText;

  /// <summary> ”казывает игровой скрипт </summary>
  public string ExpectedAnswer;

  /// <summary> ”казывает игровой скрипт </summary>
  public Dictionary<string, object> SpecificData;

  public QuestionData(string questionText, string expectedAnswer, Dictionary<string, object> specificData)
  {
    QuestionText = questionText;
    ExpectedAnswer = expectedAnswer;
    SpecificData = specificData;

    QuestionNumber = -1;
  }
}

  // при добавлении большого числа полей от контроллера - стоит разбить на 2 сущности
  public struct AnswerData
{
  /// <summary> ”казывает игровой менеджер </summary>
  public int QuestionNumber;

  /// <summary> ”казывает игровой скрипт </summary>
  public string AnswerText;

  /// <summary> ”казывает игровой скрипт </summary>
  public bool IsCorrect;

  /// <summary> ”казывает игровой менеджер </summary>
  public float ReactionTime;

  public AnswerData(string answerText, bool isCorrect)
  {
    AnswerText = answerText;
    IsCorrect = isCorrect;

    QuestionNumber = -1;
    ReactionTime = -1f;
  }
}
