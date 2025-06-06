using System.Collections.Generic;

// файл статистики (агрегированных данных)


/// <summary> Хранит среднее по очкам </summary>
[System.Serializable]
public class DataStatistics
{
  public int AverageScore;
}

/// <summary> Хранит агрегированные данные за дни/недели/месяцы <br/>
/// даты -> данные </summary>
public class PeriodStatistics
{
  public Dictionary<string, DataStatistics> DailyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> WeeklyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> MonthlyAverage = new Dictionary<string, DataStatistics>();
}

/// <summary> Хранит статистику по одной игре и дату последнего обновления </summary>
[System.Serializable]
public class GameStatisticsEntry
{
  /// <summary> Когда данные этой игры в последний раз обновлялись </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> Агрегированные данные по периодам </summary>
  public PeriodStatistics Statistics = new PeriodStatistics();
}

/// <summary> Хранит все агрегированные данные по всем играм </summary>
[System.Serializable]
public class GameStatistics
{
  /// <summary> Для избежания повторных пересчётов старых данных </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> категории -> игры -> даты -> данные </summary>
  public Dictionary<string, Dictionary<string, GameStatisticsEntry>> GamesStatistics = new Dictionary<string, Dictionary<string, GameStatisticsEntry>>();
}
