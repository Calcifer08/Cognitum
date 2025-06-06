using System.Collections.Generic;

// ���� ���������� (�������������� ������)


/// <summary> ������ ������� �� ����� </summary>
[System.Serializable]
public class DataStatistics
{
  public int AverageScore;
}

/// <summary> ������ �������������� ������ �� ���/������/������ <br/>
/// ���� -> ������ </summary>
public class PeriodStatistics
{
  public Dictionary<string, DataStatistics> DailyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> WeeklyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> MonthlyAverage = new Dictionary<string, DataStatistics>();
}

/// <summary> ������ ���������� �� ����� ���� � ���� ���������� ���������� </summary>
[System.Serializable]
public class GameStatisticsEntry
{
  /// <summary> ����� ������ ���� ���� � ��������� ��� ����������� </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> �������������� ������ �� �������� </summary>
  public PeriodStatistics Statistics = new PeriodStatistics();
}

/// <summary> ������ ��� �������������� ������ �� ���� ����� </summary>
[System.Serializable]
public class GameStatistics
{
  /// <summary> ��� ��������� ��������� ���������� ������ ������ </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> ��������� -> ���� -> ���� -> ������ </summary>
  public Dictionary<string, Dictionary<string, GameStatisticsEntry>> GamesStatistics = new Dictionary<string, Dictionary<string, GameStatisticsEntry>>();
}
