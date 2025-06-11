using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для хранения и доступа к данным об играх.
/// Загружает GamesData один раз и предоставляет вспомогательные методы.
/// </summary>
public static class GameDataManager
{
  private static GamesData _gamesData;

  private static Dictionary<string, string> _gameIdToSceneName = new();

  private static HashSet<string> _allSceneNames = new();

  public static bool IsInitialized { get; private set; } = false;

  public static void Initialize(GamesData gamesData)
  {
    if (IsInitialized)
      return;

    _gamesData = gamesData;

    foreach (var category in gamesData.Categories)
    {
      foreach (var game in category.Games)
      {
        _gameIdToSceneName[game.GameId] = game.GameScene;
        _allSceneNames.Add(game.GameScene);
      }
    }

    IsInitialized = true;
  }

  public static GamesData GetGamesData()
  {
    if (_gamesData == null)
    {
      Debug.LogError("GameDataManager не инициализирован! Не удалось загрузить список категорий.");
    }

    return _gamesData;
  }

  public static string GetSceneNameByID(string gameId)
  {
    if (_gameIdToSceneName.TryGetValue(gameId, out var sceneName))
    {
      return sceneName;
    }

    Debug.LogError($"GameId '{gameId}' не найден в GamesData.");
    return null;
  }

  public static bool IsGameScene(string sceneName)
  {
    return _allSceneNames.Contains(sceneName);
  }
}
