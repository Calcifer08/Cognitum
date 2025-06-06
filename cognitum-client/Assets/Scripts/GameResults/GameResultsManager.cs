using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultsManager
{
  // Количество правильных и неправильных ответов
  private static int _correctAnswers = 0;
  private static int _incorrectAnswers = 0;
  private static int _skippedAnswers = 0;

  // Количество успешно пройденных и не пройденных уровней
  private static int _winLevels = 0;
  private static int _failLevels = 0;

  // Минимальный и максимальный достигнутый уровень
  private static int _minLevelReached = 1;
  private static int _maxLevelReached = 1;

  // Очки, набранные за сессию
  private static int _totalScore = 0;
  private static int _maxScore = 0;

  /// <summary>
  /// Задаёт начальные значения.
  /// </summary>
  public static void InitResults(int level)
  {
    ResetResults();
    _minLevelReached = level;
    _maxLevelReached = level;
  }

  /// <summary>
  /// Увеличивает количество правильных ответов.
  /// </summary>
  public static void AddCorrectAnswer()
  {
    _correctAnswers++;
  }

  /// <summary>
  /// Увеличивает количество неправильных ответов.
  /// </summary>
  public static void AddIncorrectAnswer()
  {
    _incorrectAnswers++;
  }

  public static void AddSkippedAnswer()
  {
    _skippedAnswers++;
  }

  /// <summary>
  /// Добавляет успешное прохождение уровня и обновляет максимальный/минимальный достигнутый уровень.
  /// </summary>
  public static void AddWinLevel(int level)
  {
    _winLevels++;
    UpdateLevelBounds(level);
  }

  /// <summary>
  /// Добавляет неуспешное прохождение уровня и обновляет максимальный/минимальный достигнутый уровень.
  /// </summary>
  public static void AddFailLevel(int level)
  {
    _failLevels++;
    UpdateLevelBounds(level);
  }

  /// <summary>
  /// Обновляет границы минимального и максимального уровней.
  /// </summary>
  private static void UpdateLevelBounds(int level)
  {
    if (level < _minLevelReached) _minLevelReached = level;
    if (level > _maxLevelReached) _maxLevelReached = level;
  }

  /// <summary>
  /// Добавляет очки.
  /// </summary>
  public static void SetScore(int score)
  {
    _totalScore = score;
  }

  public static void SetMaxScore(int score)
  {
    _maxScore = score;
  }

  // Методы для получения текущих значений статистики
  public static int GetCorrectAnswers() => _correctAnswers;
  public static int GetIncorrectAnswers() => _incorrectAnswers;
  public static int GetSkippedAnswers() => _skippedAnswers;
  public static int GetWinLevels() => _winLevels;
  public static int GetFailLevels() => _failLevels;
  public static int GetMinLevelReached() => _minLevelReached;
  public static int GetMaxLevelReached() => _maxLevelReached;
  public static int GetTotalScore() => _totalScore;
  public static int GetMaxScore() => _maxScore;

  /// <summary>
  /// Выводит результаты статистики.
  /// </summary>
  public static string GetResults()
  {
    string results = $"Набрано очков: {_totalScore}\n";

    if (_totalScore <= _maxScore)
      results += $"Рекорд: {_maxScore}\n";

    results += $"Максимальный уровень: {_maxLevelReached}\n";
    results += $"Минимальный уровень: {_minLevelReached}\n";
    results += $"Пройдено уровней: {_winLevels}\n";
    results += $"Провалено уровней: {_failLevels}\n";

    float accuracy = (_correctAnswers * 100f) / (_correctAnswers + _incorrectAnswers + _skippedAnswers);
    accuracy = Mathf.Round(accuracy * 100f) / 100f;
    results += $"Точность: {accuracy}%\n";
    results += $"Правильных ответов: {_correctAnswers}\n";
    results += $"Неправильных ответов: {_incorrectAnswers}\n";
    results += $"Пропущенные ответы: {_skippedAnswers}";

    return results;
  }

  /// <summary>
  /// Возвращает достижение игрока по приоритету
  /// </summary>
  public static string GetAchievement()
  {
    if (_totalScore > _maxScore)
      return "Новый рекорд!";

    int totalAnswers = _correctAnswers + _incorrectAnswers + _skippedAnswers;

    if (totalAnswers > 0)
    {
      float accuracy = (_correctAnswers * 100f) / totalAnswers;

      if (accuracy >= 85f)
        return "Отличная точность!";
    }

    return "Хорошая работа!";
  }

  /// <summary>
  /// Сбрасывает все значения статистики.
  /// </summary>
  public static void ResetResults()
  {
    _correctAnswers = 0;
    _incorrectAnswers = 0;
    _skippedAnswers = 0;
    _winLevels = 0;
    _failLevels = 0;
    _minLevelReached = 1;
    _maxLevelReached = 1;
    _totalScore = 0;
    _maxScore = 0;
  }
}
