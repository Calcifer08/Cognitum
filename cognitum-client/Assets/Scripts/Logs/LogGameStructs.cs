using System.Collections.Generic;

// ��������� ������, ������������ ��� ����������� ������� ������
// ������������ GameManager � LogManager

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
  /// <summary> ��������� ������� �������� </summary>
  public int QuestionNumber;

  /// <summary> ��������� ������� ������ </summary>
  public string QuestionText;

  /// <summary> ��������� ������� ������ </summary>
  public string ExpectedAnswer;

  /// <summary> ��������� ������� ������ </summary>
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
  /// <summary> ��������� ������� �������� </summary>
  public int QuestionNumber;

  /// <summary> ��������� ������� ������ </summary>
  public string AnswerText;

  /// <summary> ��������� ������� ������ </summary>
  public bool IsCorrect;

  /// <summary> ��������� ������� �������� </summary>
  public float ReactionTime;

  public AnswerData(string answerText, bool isCorrect)
  {
    AnswerText = answerText;
    IsCorrect = isCorrect;

    QuestionNumber = -1;
    ReactionTime = -1f;
  }
}
