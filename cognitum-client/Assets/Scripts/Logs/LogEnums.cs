public enum StatusLevelList
{
  Win,
  Fail,
  TimeDone,
  _ // заглушка для стартового уровня
}

public enum StatusQuestionList
{
  Win,
  Fail,
  TimeDone
}

public enum TagsList
{
  // Теги для вопросов и ответов
  QuestionStart,          // Начало показа вопроса
  QuestionEnd,            // Конец показа вопроса
  MemorizePhaseStart,     // Начало фазы запоминания
  MemorizePhaseEnd,       // Конец фазы запоминания
  ForgetPhaseStart,       // Начало фазы запоминания (когда вопрос скрыт)
  ForgetPhaseEnd,         // Конец фазы запоминания (когда вопрос скрыт)
  AnswerPhaseStart,       // Начало фазы ответа
  AnswerPhaseEnd,         // Конец фазы ответа
  AnswerSubmitted,        // Пользователь отправил ответ

  // Теги для системных событий
  GameStart,              // Начало игры
  GameEnd,                // Конец игры
  LevelStart,             // Начало уровня
  LevelEnd,               // Конец уровня
  PauseStart,             // Игра приостановлена
  PauseEnd,               // Игра продолжена
}

