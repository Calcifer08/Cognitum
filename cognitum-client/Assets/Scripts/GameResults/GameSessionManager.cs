using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;

public static class GameSessionManager
{
  public const string FileName = "SessionsData.json";
  private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);
  private static SessionsData _sessionData;

  public static async Task InitializeAsync()
  {
    if (_sessionData == null)
    {
      await LoadDataAsync();
    }
  }

  private static async Task LoadDataAsync()
  {
    if (File.Exists(FilePath))
    {
      string json = await File.ReadAllTextAsync(FilePath);
      _sessionData = JsonConvert.DeserializeObject<SessionsData>(json);
      Debug.Log("Файл сессий загружен");
    }
    else
    {
      _sessionData = new SessionsData();
      Debug.Log("Файл сессий не найден. Создан новый");
    }
  }

  public static SessionsData GetSessionsData()
  {
    return _sessionData;
  }

  public static async Task SaveSessionDataAsync(string nameGame, int score, int level)
  {
    string date = System.DateTime.Now.ToString("yyyy-MM-dd");

    if (!_sessionData.Sessions.ContainsKey(nameGame))
    {
      _sessionData.Sessions[nameGame] = new Dictionary<string, List<GameSession>>();
    }

    if (!_sessionData.Sessions[nameGame].ContainsKey(date))
    {
      _sessionData.Sessions[nameGame][date] = new List<GameSession>();
    }

    var gameSession = new GameSession { Score = score, Level = level };
    _sessionData.Sessions[nameGame][date].Add(gameSession);

    await SaveFileAsync();
  }

  public static async Task SaveFileAsync()
  {
#if !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_sessionData, Formatting.None);
#elif UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_sessionData, Formatting.Indented);
#endif
    await File.WriteAllTextAsync(FilePath, json);
    Debug.Log("Данные сессии сохранены в файл");
  }
}

