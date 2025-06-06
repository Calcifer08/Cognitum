using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// события на уровне
[System.Serializable]
public class LogLevelEvents
{
  [JsonConverter(typeof(StringEnumConverter))] // JsonConverter для конвертации Enum в строку, иначе будут инты
  public TagsList Tags; // по идее будет только один тег, но если нет, то выше

  public string Time;

  public AbstractEventData EventData;

  public LogLevelEvents(TagsList tags, string time, AbstractEventData eventData)
  {
    Tags = tags;
    Time = time;
    EventData = eventData;
  }
}



// абстрактный класс, ибо у меня много классов может быть и принимать проще такой
[System.Serializable]
public abstract class AbstractEventData { }



// событие показа вопроса для матрицы
[System.Serializable]
public class EventQuestionStart : AbstractEventData
{
  public int QuestionNumber; // выдаём в контроллере
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

// событие конца показа вопроса
[System.Serializable]
public class EventQuestionEnd : AbstractEventData
{
  public int QuestionNumber;

  public EventQuestionEnd(int questionNumber) { QuestionNumber = questionNumber; }
}


// Начало фазы запоминания — когда игрок видит вопрос, но не может отвечать
public class EventMemorizePhaseStart : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса
  public float Duration; // Время запоминания

  public EventMemorizePhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

// Конец фазы забывания (Retention phase)
public class EventMemorizePhaseEnd : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса

  public EventMemorizePhaseEnd(int questionNumber)
  {
    QuestionNumber = questionNumber;
  }
}


// Начало фазы забывания (Retention phase) — когда игрок не видит вопрос и не может отвечать
public class EventForgetPhaseStart : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса
  public float Duration; // Время забывания (т.е. сколько времени пройдёт от скрытия вопроса до возможности ответа)

  public EventForgetPhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

// Конец фазы забывания (Retention phase)
public class EventForgetPhaseEnd : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса

  public EventForgetPhaseEnd(int questionNumber)
  {
    QuestionNumber = questionNumber;
  }
}


// Начало фазы ответа (когда игрок может отвечать на вопрос)
public class EventAnswerPhaseStart : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса
  public float Duration; // время фазы, указать 0, если бесконечное время

  public EventAnswerPhaseStart(int questionNumber, float duration)
  {
    QuestionNumber = questionNumber;
    Duration = duration;
  }
}

// Конец фазы ответа (когда игрок может отвечать на вопрос)
public class EventAnswerPhaseEnd : AbstractEventData
{
  public int QuestionNumber; // Номер вопроса

  [JsonConverter(typeof(StringEnumConverter))] // JsonConverter для конвертации Enum в строку, иначе будут инты
  public StatusQuestionList Status;

  public EventAnswerPhaseEnd(int questionNumber, StatusQuestionList status)
  {
    QuestionNumber = questionNumber;
    Status = status;
  }
}


// событие ответа пользователя
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



// событие паузы
[System.Serializable]
public class EventPauseStart : AbstractEventData
{
  public int PauseNumber;
  public string Description; // ибо можно в игре или при сворачивании

  public EventPauseStart(int pauseNumber, string description)
  {
    PauseNumber = pauseNumber;
    Description = description;
  }
}

// событие паузы
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




// системные события
[System.Serializable]
public class EventSystem : AbstractEventData
{
  public string EventType; // тип системного события
  public string Description; // описание

  public EventSystem(string eventType, string description) { EventType = eventType; Description = description; }
}

