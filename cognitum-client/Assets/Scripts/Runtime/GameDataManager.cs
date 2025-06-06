using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для хранения и доступа к данным об играх.
/// Загружает GamesData один раз и предоставляет вспомогательные методы.
/// </summary>
public static class GameDataManager
{
  private static GamesData _gamesData;

  // Словарь для быстрого доступа: GameId -> имя сцены
  private static Dictionary<string, string> _gameIdToSceneName = new();

  // Хешсет всех имён сцен для быстрой проверки
  private static HashSet<string> _allSceneNames = new();

  // Флаг инициализации
  public static bool IsInitialized { get; private set; } = false;

  /// <summary>
  /// Инициализация менеджера — сохраняем GamesData и заполняем вспомогательные словари.
  /// </summary>
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

  /// <summary>
  /// Получить ссылку на GamesData, если нужно вручную обойти категории и игры.
  /// </summary>
  public static GamesData GetGamesData()
  {
    if (_gamesData == null)
    {
      Debug.LogError("GameDataManager не инициализирован! Не удалось загрузить список категорий.");
    }

    return _gamesData;
  }

  /// <summary>
  /// Получить имя сцены по GameId.
  /// </summary>
  public static string GetSceneName(string gameId)
  {
    if (_gameIdToSceneName.TryGetValue(gameId, out var sceneName))
    {
      return sceneName;
    }

    Debug.LogError($"GameId '{gameId}' не найден в GamesData.");
    return null;
  }

  /// <summary>
  /// Проверить, является ли имя сцены сценой игры.
  /// </summary>
  public static bool IsGameScene(string sceneName)
  {
    return _allSceneNames.Contains(sceneName);
  }
}
