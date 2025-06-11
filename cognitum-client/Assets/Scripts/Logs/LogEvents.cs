using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// события на уровня

[System.Serializable]
public class LogLevelEvents
{
  [JsonConverter(typeof(StringEnumConverter))]
  public TagsList Tags;

  public string Time;

  public AbstractEventData EventData;

  public LogLevelEvents(TagsList tags, string time, AbstractEventData eventData)
  {
    Tags = tags;
    Time = time;
    EventData = eventData;
  }
}



[System.Serializable]
public abstract class AbstractEventData { }



[System.Serializable]
public class EventQuestionStart : AbstractEventData
{
  public int QuestionNumber;
  public string QuestionText;
  public string ExpectedAnswer;
  public Dictionary<string, object> SpecificData;

  public EventQuestionStart(
    int questionNumber, 
    string questionText, 
    string expectedAnswer, 
    Dictionary<string, object> specificData)
  {
    QuestionNumber = questionNumber;
    QuestionText = questionText;
    ExpectedAnswer = expectedAnswer;
    SpecificData = specificData;
  }
}

[System.Serializable]
public class EventQuestionEnd : AbstractEventData
{
  public int QuestionNumber;

  public EventQuestionEnd(int questionNumber) { QuestionNumber = questionNumber; }
}


public class EventMemorizePhaseStart : AbstractEventData
{
  public int QuestionNumber;
  public float Duration;

  public EventMemorizePhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

public class EventMemorizePhaseEnd : AbstractEventData
{
  public int QuestionNumber;

  public EventMemorizePhaseEnd(int questionNumber)
  {
    QuestionNumber = questionNumber;
  }
}


public class EventForgetPhaseStart : AbstractEventData
{
  public int QuestionNumber;
  public float Duration;

  public EventForgetPhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

public class EventForgetPhaseEnd : AbstractEventData
{
  public int QuestionNumber;

  public EventForgetPhaseEnd(int questionNumber)
  {
    QuestionNumber = questionNumber;
  }
}


public class EventAnswerPhaseStart : AbstractEventData
{
  public int QuestionNumber;
  public float Duration = 0; // 0 = бесконечное время

  public EventAnswerPhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

public class EventAnswerPhaseEnd : AbstractEventData
{
  public int QuestionNumber; 

  [JsonConverter(typeof(StringEnumConverter))]
  public StatusQuestionList Status;

  public EventAnswerPhaseEnd(int questionNumber, StatusQuestionList status)
  {
    QuestionNumber = questionNumber;
    Status = status;
  }
}


[System.Serializable]
public class EventAnswer : AbstractEventData
{
  public int QuestionNumber;
  public string AnswerText;
  public bool IsCorrect;
  public float ReactionTime;

  public EventAnswer(int questionNumber, string answerText, bool isCorrect, float reactionTime)
  {
    QuestionNumber = questionNumber;
    AnswerText = answerText;
    IsCorrect = isCorrect;
    ReactionTime = reactionTime;
  }
}



[System.Serializable]
public class EventPauseStart : AbstractEventData
{
  public int PauseNumber;
  public string Description;

  public EventPauseStart(int pauseNumber, string description)
  {
    PauseNumber = pauseNumber;
    Description = description;
  }
}

[System.Serializable]
public class EventPauseEnd : AbstractEventData
{
  public int PauseNumber;
  public float Duration;

  public EventPauseEnd(int pauseNumber, float duration)
  {
    PauseNumber = pauseNumber;
    Duration = duration;
  }
}




[System.Serializable]
public class EventSystem : AbstractEventData
{
  public string EventType;
  public string Description;

  public EventSystem(string eventType, string description) { EventType = eventType; Description = description; }
}

