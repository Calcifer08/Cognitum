using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GameStatisticsManager
{
  public const string FileName = "GameStatistics.json";
  private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);

  private static GameStatistics _gameStatistics; // категории -> игры -> даты -> данные

  private static string _lastUpdate;

  private static Dictionary<string, List<string>> _categoryList = new Dictionary<string, List<string>>();

  public static async Task InitializeAsync()
  {
    if (_gameStatistics == null)
    {
      InitializeCategoryList();

      await LoadGameStatisticsAsync();
    }
  }

  public static void InitializeCategoryList()
  {
    GamesData gameData = GameDataManager.GetGamesData();

    foreach (var category in gameData.Categories)
    {
      _categoryList[category.CategoryId] = new List<string>();

      foreach (var game in category.Games)
      {
        _categoryList[category.CategoryId].Add(game.GameId);
      }
    }
  }

  private static async Task LoadGameStatisticsAsync()
  {
    if (File.Exists(FilePath))
    {
      string json = await File.ReadAllTextAsync(FilePath);
      _gameStatistics = JsonConvert.DeserializeObject<GameStatistics>(json);
      Debug.Log("Файл статистики загружен");
    }
    else
    {
      _gameStatistics = new GameStatistics();
      Debug.Log("Файл статистики не найден. Создан новый");
    }
  }

  private static string GetCategoryName(string nameGame)
  {
    foreach (var category in _categoryList)
    {
      if (category.Value.Contains(nameGame))
      {
        return category.Key;
      }
    }

    return "NoneCategory";
  }

  public static async Task AggregateDateAsync()
  {
    await AggregateDailyDataAsync();
    Debug.Log("Файл сессий обработан и сохранён");

    await AggregateWeeklyAndMonthlyDataAsync();
    Debug.Log("Файл статистики игр обновлён и сохранён");
  }

  public static async Task SaveStatisticsFileAsync(GameStatistics data)
  {
    _gameStatistics = data;

#if UNITY_ANDROID && !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(data, Formatting.None);
#elif UNITY_EDITOR
    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
#endif

    await File.WriteAllTextAsync(FilePath, json);
    Debug.Log("Файл статистики игр сохранён");
  }

  private static async Task AggregateDailyDataAsync()
  {
    var sessionData = GameSessionManager.GetSessionsData();
    var today = DateTime.Now.ToString("yyyy-MM-dd");

    foreach (var session in sessionData.Sessions)
    {
      string nameGame = session.Key;

      foreach (var date in session.Value.ToList())
      {
        string dateGame = date.Key;

        if (DateTime.Parse(dateGame) < DateTime.Parse(_gameStatistics.LastUpdate).Date)
        {
          sessionData.Sessions[nameGame].Remove(dateGame);
          continue;
        }

        int averageScore = (int)Math.Round(date.Value.Average(session => session.Score));

        AddToAggregation(_gameStatistics, nameGame, dateGame, averageScore);

        if (dateGame != today)
        {
          sessionData.Sessions[nameGame].Remove(dateGame);
        }
      }
    }

    await SaveStatisticsFileAsync(_gameStatistics);
    await GameSessionManager.SaveFileAsync();
  }

  private static void AddToAggregation(GameStatistics gameAggregation, string nameGame, string dateGame, int scoreGame)
  {
    string nameCategory = GetCategoryName(nameGame);

    if (!gameAggregation.GamesStatistics.ContainsKey(nameCategory))
    {
      gameAggregation.GamesStatistics[nameCategory] = new Dictionary<string, GameStatisticsEntry>();
    }

    if (!gameAggregation.GamesStatistics[nameCategory].ContainsKey(nameGame))
    {
      gameAggregation.GamesStatistics[nameCategory][nameGame] = new GameStatisticsEntry();
    }

    GameStatisticsEntry gameEntry = gameAggregation.GamesStatistics[nameCategory][nameGame];

    if (gameEntry.Statistics.DailyAverage.TryGetValue(dateGame, out var existingData))
    {
      if (existingData.AverageScore == scoreGame)
      {
        return;
      }
    }

    gameEntry.LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    gameEntry.Statistics.DailyAverage[dateGame] = new DataStatistics { AverageScore = scoreGame };
  }

  private static async Task AggregateWeeklyAndMonthlyDataAsync()
  {
    _lastUpdate = _gameStatistics.LastUpdate;

    var updatedStatistics = new GameStatistics
    {
      LastUpdate = _lastUpdate,
      GamesStatistics = new Dictionary<string, Dictionary<string, GameStatisticsEntry>>()
    };

    foreach (var category in _gameStatistics.GamesStatistics)
    {
      foreach (var gamePair in category.Value)
      {
        GameStatisticsEntry gameEntry = gamePair.Value;
        PeriodStatistics stats = gameEntry.Statistics;

        SaveAggregatePeriod(stats.DailyAverage, stats.WeeklyAverage, 7);

        SaveAggregatePeriod(stats.DailyAverage, stats.MonthlyAverage, 30);

        RemoveOldData(stats);

        if (DateTime.Parse(gameEntry.LastUpdate) > DateTime.Parse(_lastUpdate))
        {
          if (!updatedStatistics.GamesStatistics.ContainsKey(category.Key))
          {
            updatedStatistics.GamesStatistics[category.Key] = new Dictionary<string, GameStatisticsEntry>();
          }

          updatedStatistics.GamesStatistics[category.Key][gamePair.Key] = gameEntry;
        }
      }
    }

    _gameStatistics.LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    await SaveStatisticsFileAsync(_gameStatistics);

    if (updatedStatistics.GamesStatistics.Count > 0)
    {
      await SendPartialStatisticsAsync(updatedStatistics);
    }
  }

  private static void SaveAggregatePeriod(
    Dictionary<string, DataStatistics> sourceData,
    Dictionary<string, DataStatistics> targetData,
    int periodDays)
  {
    var periodData = AggregatePeriod(sourceData, periodDays);

    DateTime periodEndDate;

    foreach (var data in periodData)
    {
      if (periodDays == 7)
      {
        periodEndDate = DateTime.Parse(data.Key).AddDays(6);
      }
      else if (periodDays == 30)
      {
        periodEndDate = DateTime.Parse(data.Key).AddMonths(1).AddDays(-1);
      }
      else
      {
        throw new ArgumentException("Неверно указан период");
      }

      if (periodEndDate >= DateTime.Parse(_lastUpdate).Date)
      {
        targetData[data.Key] = new DataStatistics { AverageScore = data.Value };
      }
    }
  }

  private static Dictionary<string, int> AggregatePeriod(Dictionary<string, DataStatistics> data, int days)
  {
    var result = new Dictionary<string, int>();

    var groupData = data
      .GroupBy(g =>
      {
        var date = DateTime.Parse(g.Key);

        if (days == 7)
        {
          return date.AddDays(-(int)date.DayOfWeek).Date;
        }
        else if (days == 30)
        {
          return new DateTime(date.Year, date.Month, 1).Date;
        }
        else
        {
          throw new ArgumentException("Неверно указан период");
        }

      })
      .Select(s => new
      {
        period = s.Key.ToString("yyyy-MM-dd"),
        averageScore = (int)Math.Round(s.Average(a => a.Value.AverageScore))
      });

    foreach (var entryData in groupData)
    {
      result[entryData.period] = entryData.averageScore;
    }

    return result;
  }

  private static void RemoveOldData(PeriodStatistics gameData)
  {
    gameData.DailyAverage = gameData.DailyAverage
        .OrderByDescending(data => DateTime.Parse(data.Key))
        .Take(35)
        .ToDictionary(data => data.Key, data => data.Value);

    gameData.WeeklyAverage = gameData.WeeklyAverage
        .OrderByDescending(data => DateTime.Parse(data.Key))
        .Take(8)
        .ToDictionary(data => data.Key, data => data.Value);

    gameData.MonthlyAverage = gameData.MonthlyAverage
        .OrderByDescending(data => DateTime.Parse(data.Key))
        .Take(8)
        .ToDictionary(data => data.Key, data => data.Value);
  }

  private static int CalculateMedian(IEnumerable<int> scores)
  {
    var sortedScores = scores.OrderBy(x => x).ToList();
    int count = sortedScores.Count;

    if (count == 0)
      return 0;

    if (count % 2 == 1)
    {
      return sortedScores[count / 2];
    }
    else
    {
      int mid1 = sortedScores[(count / 2) - 1];
      int mid2 = sortedScores[count / 2];
      return (int)Math.Round((mid1 + mid2) / 2.0);
    }
  }

  public static async Task SendPartialStatisticsAsync(GameStatistics partialStatistics)
  {
    string json = JsonConvert.SerializeObject(partialStatistics, Formatting.Indented);

    bool isSuccess = await SendGameStatisticsAsync(json);

    if (!isSuccess)
    {
      _ = DataSyncManager.AddFileToQueueAsync(FileName);
    }
  }

  public static async Task<bool> SendAllStatisticsAsync()
  {
    if (_gameStatistics == null)
    {
      Debug.LogError("Нет статистики для отправки.");
      return false;
    }

    string json = JsonConvert.SerializeObject(_gameStatistics, Formatting.None);

    return await SendGameStatisticsAsync(json);
  }

  private static async Task<bool> SendGameStatisticsAsync(string json, int retryCount = 0)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.Log("Нет интернета. Статистика будет отправлена позже.");
      return false;
    }

    string token = AuthManager.GetAccessToken();
    if (string.IsNullOrEmpty(token))
    {
      Debug.LogError("Нет токена для отправки статистики.");
      AuthManager.ClearTokensAndLogout();
      return false;
    }

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.SaveGameStatisticksUrl, "POST"))
    {
      byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
      request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Authorization", $"Bearer {token}");
      request.SetRequestHeader("Content-Type", "application/json");

      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        Debug.Log("Статистика успешно отправлена на сервер.");
        return true;
      }
      else
      {
        return await HandleSendStatisticsErrorAsync(request, json, retryCount);
      }
    }
  }

  private static async Task<bool> HandleSendStatisticsErrorAsync(UnityWebRequest request, string json, int retryCount)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("Сервер не отвечает. Статистика будет отправлена позже.");
      return false;
    }

    string parsedMessage = ErrorResponse.ParseErrorMessage(request.downloadHandler.text);

    if (parsedMessage == "Токен истёк")
    {
      if (retryCount < 1)
      {
        Debug.Log("Токен истёк. Пробуем обновить...");
        await TokenRefreshManager.RefreshTokenAsync();

        if (!string.IsNullOrEmpty(SecureStorage.GetData(APIConstants.StorageKeys.AccessToken)))
        {
          return await SendGameStatisticsAsync(json, retryCount + 1);
        }
        else
        {
          Debug.LogError("Не удалось обновить токен. Отправка статистики отменена.");
          return false;
        }
      }
      else
      {
        Debug.LogError("Превышено количество попыток обновления токена.");
        return false;
      }
    }
    else if (request.responseCode == 400)
    {
      Debug.LogError($"Ошибка: некорректные данные. {parsedMessage}");
      return false;
    }
    else if (request.responseCode == 401)
    {
      Debug.LogError($"Ошибка авторизации: {parsedMessage} (код {request.responseCode}). Пользователь будет выведен из системы.");
      AuthManager.ClearTokensAndLogout();
      return false;
    }
    else
    {
      Debug.LogError($"Неизвестная ошибка при отправке статистики: {parsedMessage} (код {request.responseCode})");
      return false;
    }
  }




  public static Dictionary<string, int> GetPeriodData(string category, string gameId, string periodName)
  {
    if (!_gameStatistics.GamesStatistics.TryGetValue(category, out var gamesInCategory))
    {
      Debug.LogWarning($"Нет данных по категории: {category}");
      return new Dictionary<string, int>();
    }

    if (!gamesInCategory.TryGetValue(gameId, out var periodStatistics))
    {
      Debug.LogWarning($"Нет данных по игре: {gameId} в категории: {category}");
      return new Dictionary<string, int>();
    }

    Dictionary<string, DataStatistics> rawPeriodData;
    switch (periodName)
    {
      case "DailyAverage":
        rawPeriodData = periodStatistics.Statistics.DailyAverage;
        break;
      case "WeeklyAverage":
        rawPeriodData = periodStatistics.Statistics.WeeklyAverage;
        break;
      case "MonthlyAverage":
        rawPeriodData = periodStatistics.Statistics.MonthlyAverage;
        break;
      default:
        Debug.LogWarning($"Неизвестный тип периода: {periodName}");
        return new Dictionary<string, int>();
    }

    if (rawPeriodData == null || rawPeriodData.Count == 0)
    {
      Debug.LogWarning($"Нет данных по периоду {periodName} для игры {gameId} в категории {category}");
      return new Dictionary<string, int>();
    }

    Dictionary<string, int> result = new Dictionary<string, int>();
    foreach (var entry in rawPeriodData)
    {
      result[entry.Key] = entry.Value.AverageScore;
    }

    return result;
  }
}
