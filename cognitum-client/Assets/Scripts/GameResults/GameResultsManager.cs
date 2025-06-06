using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultsManager
{
  // ���������� ���������� � ������������ �������
  private static int _correctAnswers = 0;
  private static int _incorrectAnswers = 0;
  private static int _skippedAnswers = 0;

  // ���������� ������� ���������� � �� ���������� �������
  private static int _winLevels = 0;
  private static int _failLevels = 0;

  // ����������� � ������������ ����������� �������
  private static int _minLevelReached = 1;
  private static int _maxLevelReached = 1;

  // ����, ��������� �� ������
  private static int _totalScore = 0;
  private static int _maxScore = 0;

  /// <summary>
  /// ����� ��������� ��������.
  /// </summary>
  public static void InitResults(int level)
  {
    ResetResults();
    _minLevelReached = level;
    _maxLevelReached = level;
  }

  /// <summary>
  /// ����������� ���������� ���������� �������.
  /// </summary>
  public static void AddCorrectAnswer()
  {
    _correctAnswers++;
  }

  /// <summary>
  /// ����������� ���������� ������������ �������.
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
  /// ��������� �������� ����������� ������ � ��������� ������������/����������� ����������� �������.
  /// </summary>
  public static void AddWinLevel(int level)
  {
    _winLevels++;
    UpdateLevelBounds(level);
  }

  /// <summary>
  /// ��������� ���������� ����������� ������ � ��������� ������������/����������� ����������� �������.
  /// </summary>
  public static void AddFailLevel(int level)
  {
    _failLevels++;
    UpdateLevelBounds(level);
  }

  /// <summary>
  /// ��������� ������� ������������ � ������������� �������.
  /// </summary>
  private static void UpdateLevelBounds(int level)
  {
    if (level < _minLevelReached) _minLevelReached = level;
    if (level > _maxLevelReached) _maxLevelReached = level;
  }

  /// <summary>
  /// ��������� ����.
  /// </summary>
  public static void SetScore(int score)
  {
    _totalScore = score;
  }

  public static void SetMaxScore(int score)
  {
    _maxScore = score;
  }

  // ������ ��� ��������� ������� �������� ����������
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
  /// ������� ���������� ����������.
  /// </summary>
  public static string GetResults()
  {
    string results = $"������� �����: {_totalScore}\n";

    if (_totalScore <= _maxScore)
      results += $"������: {_maxScore}\n";

    results += $"������������ �������: {_maxLevelReached}\n";
    results += $"����������� �������: {_minLevelReached}\n";
    results += $"�������� �������: {_winLevels}\n";
    results += $"��������� �������: {_failLevels}\n";

    float accuracy = (_correctAnswers * 100f) / (_correctAnswers + _incorrectAnswers + _skippedAnswers);
    accuracy = Mathf.Round(accuracy * 100f) / 100f;
    results += $"��������: {accuracy}%\n";
    results += $"���������� �������: {_correctAnswers}\n";
    results += $"������������ �������: {_incorrectAnswers}\n";
    results += $"����������� ������: {_skippedAnswers}";

    return results;
  }

  /// <summary>
  /// ���������� ���������� ������ �� ����������
  /// </summary>
  public static string GetAchievement()
  {
    if (_totalScore > _maxScore)
      return "����� ������!";

    int totalAnswers = _correctAnswers + _incorrectAnswers + _skippedAnswers;

    if (totalAnswers > 0)
    {
      float accuracy = (_correctAnswers * 100f) / totalAnswers;

      if (accuracy >= 85f)
        return "�������� ��������!";
    }

    return "������� ������!";
  }

  /// <summary>
  /// ���������� ��� �������� ����������.
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
