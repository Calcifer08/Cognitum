using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class PlayerDataManager
{
  private static PlayerData _playerData;
  public const string FileName = "PlayerData.json";
  private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);

  public static async Task InitializeAsync()
  {
    if (_playerData == null)
    {
      await LoadPlayerDataAsync();
    }
  }

  private static async Task LoadPlayerDataAsync()
  {
    if (File.Exists(FilePath))
    {
      string json = await File.ReadAllTextAsync(FilePath);
      _playerData = JsonConvert.DeserializeObject<PlayerData>(json);
      Debug.Log("Файл профиля игрока загружен");
    }
    else
    {
      string userId = GetUserIdFromToken(AuthManager.GetAccessToken());
      _playerData = new PlayerData(userId);
      await SavePlayerDataAsync(_playerData, false);
      Debug.Log("Файл профиля игрока не найден. Создан новый");
    }
  }

  public static PlayerData GetPlayerData()
  {
    return _playerData;
  }

  /// <summary>
  /// Извлекает userId из JWT-токена.
  /// </summary>
  private static string GetUserIdFromToken(string token)
  {
    if (token == null)
    {
      Debug.LogError("JWT отсутствует");
      return null;
    }

    try
    {
      var parts = token.Split('.');
      if (parts.Length != 3)
        throw new ArgumentException("Неверный JWT токен");

      string payload = Base64UrlDecode(parts[1]);
      var payloadObj = JsonConvert.DeserializeObject<JObject>(payload);

      if (payloadObj?["userId"] == null)
      {
        throw new ArgumentException("Токен не содержит userId");
      }

      return payloadObj["userId"].ToString();
    }
    catch (Exception e)
    {
      Debug.LogError("Ошибка при разборе JWT: " + e.Message);
      throw;
    }
  }

  private static string Base64UrlDecode(string input)
  {
    string base64 = input.Replace('-', '+').Replace('_', '/');
    switch (base64.Length % 4)
    {
      case 2: base64 += "=="; break;
      case 3: base64 += "="; break;
    }
    return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
  }

  public static async Task SavePlayerDataAsync(PlayerData playerData, bool isSendToServer)
  {
    if (playerData == null)
      return;

    _playerData = playerData;

#if !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_playerData, Formatting.None);
#elif UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_playerData, Formatting.Indented);
#endif

    await File.WriteAllTextAsync(FilePath, json);
    Debug.Log("Данные игрока сохранены локально.");

    if (isSendToServer)
    {
      bool isSuccess = await SendProfileDataAsync(json);

      if (!isSuccess)
      {
        _ = DataSyncManager.AddFileToQueueAsync(FileName);
      }
    }
  }

  private static async Task<bool> SendProfileDataAsync(string json, int retryCount = 0)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.Log("Нет интернета, данные игрока будут отправлены позже");
      return false;
    }

    string token = AuthManager.GetAccessToken();
    if (string.IsNullOrEmpty(token))
    {
      Debug.Log("Ошибка: нет токена для отправки данных игрока");
      AuthManager.ClearTokensAndLogout();
      return false;
    }

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.SavePlayerDataUrl, "POST"))
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
        Debug.Log("Данные игрока успешно отправлены на сервер.");
        return true;
      }
      else
      {
        return await HandleSaveDataErrorAsync(request, json, retryCount);
      }
    }
  }

  private static async Task<bool> HandleSaveDataErrorAsync(UnityWebRequest request, string json, int retryCount)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("Ошибка: сервер не отвечает. Данные игрока будут отправлены позже.");
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
          return await SendProfileDataAsync(json, retryCount + 1);
        }
        else
        {
          Debug.LogError("Ошибка: не удалось обновить токены, отправка данных прервана.");
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

  public static async Task<bool> SendPlayerDataFromQueueAsync()
  {
    if (_playerData == null)
    {
      Debug.LogError("Нет данных игрока для отправки.");
      return false;
    }

    string json = JsonConvert.SerializeObject(_playerData, Formatting.None);
    bool isSuccess = await SendProfileDataAsync(json);

    return isSuccess;
  }
}
