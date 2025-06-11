using System.Collections.Generic;

// GameStatistics.json


/// <summary> —реднее по очкам </summary>
[System.Serializable]
public class DataStatistics
{
  public int AverageScore;
}

/// <summary> јгрегированные данные за дни/недели/мес€цы <br/>
/// даты -> данные </summary>
public class PeriodStatistics
{
  public Dictionary<string, DataStatistics> DailyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> WeeklyAverage = new Dictionary<string, DataStatistics>();
  public Dictionary<string, DataStatistics> MonthlyAverage = new Dictionary<string, DataStatistics>();
}

/// <summary> —татистика по одной игре и дата последнего обновлени€ </summary>
[System.Serializable]
public class GameStatisticsEntry
{
  /// <summary>  огда данные этой игры в последний раз обновл€лись </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> јгрегированные данные по периодам </summary>
  public PeriodStatistics Statistics = new PeriodStatistics();
}

/// <summary> ¬се агрегированные данные по всем играм </summary>
[System.Serializable]
public class GameStatistics
{
  /// <summary> ƒл€ избежани€ повторных пересчЄтов старых данных </summary>
  public string LastUpdate = "2024-01-01 10:00";

  /// <summary> категории -> игры -> даты -> данные </summary>
  public Dictionary<string, Dictionary<string, GameStatisticsEntry>> GamesStatistics = new Dictionary<string, Dictionary<string, GameStatisticsEntry>>();
}
