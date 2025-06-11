using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GameConfigManager
{
  public const string FileName = "ConfigGame.json";
  private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);
  private static GameConfigData _gamesConfigData;

  public static async Task InitializeAsync()
  {
    if (_gamesConfigData == null)
    {
      await LoadDataAsync();
    }
  }

  private static async Task LoadDataAsync()
  {
    if (File.Exists(FilePath))
    {
      string json = await File.ReadAllTextAsync(FilePath);
      _gamesConfigData = JsonConvert.DeserializeObject<GameConfigData>(json);
      Debug.Log("Файл конфигов игр загружен");
    }
    else
    {
      _gamesConfigData = new GameConfigData();
      Debug.Log("Файл конфигов игр не найден. Создан новый");
    }
  }

  public static GameConfigData GetGamesConfigData()
  {
    return _gamesConfigData;
  }

  public static GameConfig GetGameConfig(string nameGame)
  {
    if (_gamesConfigData.GamesConfigData.ContainsKey(nameGame))
    {
      GameConfig gameConfig = _gamesConfigData.GamesConfigData[nameGame];
      Debug.Log($"Данные игры {nameGame} загружены. Уровень {gameConfig.CurrentLevel}");
      return gameConfig;
    }
    else
    {
      Debug.Log($"Данные игры {nameGame} отсутствуют");
      return new GameConfig();
    }
  }

  public static async Task UpdateConfigForGameAsync(string nameGame, int level, int countGame, int score)
  {
    if (_gamesConfigData.GamesConfigData.ContainsKey(nameGame))
    {
      _gamesConfigData.GamesConfigData[nameGame].CurrentLevel = level;

      if (level > _gamesConfigData.GamesConfigData[nameGame].MaxLevelReached)
      {
        _gamesConfigData.GamesConfigData[nameGame].MaxLevelReached = level;
      }

      if (score > _gamesConfigData.GamesConfigData[nameGame].MaxScore)
      {
        _gamesConfigData.GamesConfigData[nameGame].MaxScore = score;
      }

      _gamesConfigData.GamesConfigData[nameGame].CountGame = countGame;
      Debug.Log($"Данные игры {nameGame} обновлены. Уровень {level}, CountGame: {countGame}");
    }
    else
    {
      _gamesConfigData.GamesConfigData[nameGame] =
        new GameConfig() { CurrentLevel = level, MaxLevelReached = level, CountGame = countGame, MaxScore = score };
      Debug.Log($"Данные игры {nameGame} созданы. Уровень {level}, CountGame: {countGame}");
    }

    await SaveGameConfigAsync(_gamesConfigData);

    var updatedGameConfig = new GameConfigData();
    updatedGameConfig.GamesConfigData[nameGame] = _gamesConfigData.GamesConfigData[nameGame];
    await SendSingleGameConfigAsync(updatedGameConfig);
  }

  public static async Task SaveGameConfigAsync(GameConfigData gamesConfigData)
  {
    _gamesConfigData = gamesConfigData;

#if !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(gamesConfigData, Formatting.None);
#elif UNITY_EDITOR
    string json = JsonConvert.SerializeObject(gamesConfigData, Formatting.Indented);
#endif
    await File.WriteAllTextAsync(FilePath, json);
    Debug.Log("Данные всех игр сохранены локально");
  }

  public static async Task SendSingleGameConfigAsync(GameConfigData gameConfigData)
  {    
    string json = JsonConvert.SerializeObject(gameConfigData, Formatting.Indented);

    bool isSuccess = await SendGameConfigAsync(json);

    if (!isSuccess)
    {
      _ = DataSyncManager.AddFileToQueueAsync(FileName);
    }
  }

  public static async Task<bool> SendAllGameConfigAsync()
  {
    if (_gamesConfigData == null)
    {
      Debug.LogError("Нет данных игрока для отправки.");
      return false;
    }

    string json = JsonConvert.SerializeObject(_gamesConfigData, Formatting.None);

    bool isSuccess = await SendGameConfigAsync(json);

    return isSuccess;
  }

  public static async Task<bool> SendGameConfigAsync(string json, int retryCount = 0)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.Log("Нет интернета, данные конфигурации игр будут отправлены позже");
      return false;
    }

    string token = AuthManager.GetAccessToken();
    if (string.IsNullOrEmpty(token))
    {
      Debug.LogError("Ошибка: нет токена для отправки конфигурации игр");
      AuthManager.ClearTokensAndLogout();
      return false;
    }

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.SaveGameConfigUrl, "POST"))
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
        Debug.Log("Конфигурационные данные игр успешно отправлены на сервер.");
        return true;
      }
      else
      {
        return await HandleSaveGameConfigErrorAsync(request, json, retryCount);
      }
    }
  }

  private static async Task<bool> HandleSaveGameConfigErrorAsync(UnityWebRequest request, string json, int retryCount)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("Ошибка: сервер не отвечает. Данные конфигурации игр будут отправлены позже.");
      return false;
    }

    string parsedMessage = ErrorResponse.ParseErrorMessage(request.downloadHandler.text);

    if (parsedMessage == "Токен истёк")
    {
      if (retryCount < 1)
      {
        Debug.Log("Токен истёк, обновляем...");
        await TokenRefreshManager.RefreshTokenAsync();

        if (!string.IsNullOrEmpty(SecureStorage.GetData(APIConstants.StorageKeys.AccessToken)))
        {
          return await SendGameConfigAsync(json, retryCount + 1);
        }
        else
        {
          Debug.LogError("Ошибка: не удалось обновить токены, отправка данных конфигурации прервана.");
          return false;
        }
      }
      else
      {
        Debug.LogError("Токен истёк, но попытки обновления превышены.");
        return false;
      }
    }
    else if (request.responseCode == 400)
    {
      Debug.LogError($"Ошибка некорректных данных: ({parsedMessage}).");
      return false;
    }
    else if (request.responseCode == 401)
    {
      Debug.LogError($"Ошибка клиента: {parsedMessage} (код {request.responseCode}). Пользователь будет выведен из системы.");
      AuthManager.ClearTokensAndLogout();
      return false;
    }
    else
    {
      Debug.LogError($"Неизвестная ошибка: {parsedMessage} (код {request.responseCode})");
      return false;
    }
  }
}
