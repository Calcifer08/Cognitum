using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public static class AuthManager
{
  private static string _accessToken = null;

  /// <summary>
  /// Выполняет регистрацию нового пользователя.
  /// </summary>
  public static async Task RegisterAsync(string email, string password, TMP_Text statusText)
  {
    await SendAuthRequestAsync(APIConstants.ApiEndpoints.RegisterUrl, email, password, statusText);
  }

  /// <summary>
  /// Выполняет вход пользователя.
  /// </summary>
  public static async Task LoginAsync(string email, string password, TMP_Text statusText)
  {
    await SendAuthRequestAsync(APIConstants.ApiEndpoints.LoginUrl, email, password, statusText);
  }

  /// <summary>
  /// Отправляет запрос на сервер для регистрации или входа.
  /// </summary>
  private static async Task SendAuthRequestAsync(string url, string email, string password, TMP_Text statusText)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      statusText.text = "Нет интернета";
      return;
    }

    UserCredentialsRequest requestData = new UserCredentialsRequest { email = email, password = password };
    string jsonData = JsonConvert.SerializeObject(requestData);
    byte[] postData = Encoding.UTF8.GetBytes(jsonData);

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
      request.uploadHandler = new UploadHandlerRaw(postData);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json"); // т.к. оптравляем json

      // Ожидаем завершения запроса асинхронно
      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();  // Освобождаем поток для других задач
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        // Декодируем JSON-ответ
        AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);

        if (!string.IsNullOrEmpty(response.accessToken) && !string.IsNullOrEmpty(response.refreshToken))
        {
          SaveTokens(response.accessToken, response.refreshToken);
          statusText.text = "Успешный вход! \nСинхронизируем данные...";

          if (response.playerData != null)
          {
            await PlayerDataManager.SavePlayerDataAsync(response.playerData, false); // сохранение локально без отправки
          }
          else
          {
            Debug.LogError("response.playerData != null");
          }

          if (response.gameConfigData != null)
          {
            GameConfigData gameConfigData = new GameConfigData() { GamesConfigData = response.gameConfigData }; 
            await GameConfigManager.SaveGameConfigAsync(gameConfigData);
          }
          else
          {
            Debug.LogError("response.gameConfigData != null");
          }

          if (response.gameStatistics != null)
          {
           await GameStatisticsManager.SaveStatisticsFileAsync(response.gameStatistics);
          }
          else
          {
            Debug.LogError("response.gameStatistics != null");
          }

          await Task.WhenAll(
            AppSettingsManager.InitializeAsync(),
            GameSessionManager.InitializeAsync()
          );

          SceneManager.LoadScene(SceneNames.MenuCategoryGame);
        }
        else
        {
          // если сервер прислал не токены
          statusText.text = "Ошибка: некорректные данные с сервера";
        }
      }
      else
      {
        HandleAuthErrorResponse(request, statusText);
      }
    }
  }

  /// <summary>
  /// Обрабатывает ошибки, полученные от сервера при авторизации.
  /// </summary>
  private static void HandleAuthErrorResponse(UnityWebRequest request, TMP_Text statusText)
  {
    if (request.responseCode == 0)
    {
      statusText.text = "Ошибка: сервер недоступен";
      Debug.LogError("Ошибка авторизации: сервер не отвечает");
      return;
    }

    string errorMessage = request.downloadHandler.text;
    string parsedMessage = ErrorResponse.ParseErrorMessage(errorMessage);

    if (request.responseCode == 400)
    {
      // Ошибка неверных данных (например, неправильный email или пароль,или пользователь есть)
      statusText.text = $"Ошибка: {parsedMessage}";
    }
    else if (request.responseCode == 500)
    {
      // Ошибка сервера
      statusText.text = "Ошибка сервера, попробуйте позже.";
    }
    else
    {
      // Общая ошибка
      statusText.text = $"Неизвестная ошибка сервера";
      Debug.LogError($"Ошибка: {parsedMessage}");
    }

    Debug.LogError($"Ошибка авторизации ({request.responseCode}): {parsedMessage}");
  }

  /// <summary>
  /// Получает access-токен из памяти. Если его нет, то перенесёт на сцену авторизации
  /// </summary>
  public static string GetAccessToken()
  {
    if (_accessToken == null)
    {
      _accessToken = SecureStorage.GetData(APIConstants.StorageKeys.AccessToken);
    }

    if (string.IsNullOrEmpty(_accessToken))
    {
      Debug.LogWarning("Access-токен отсутствует. Перенаправляем на авторизацию.");
      SceneManager.LoadScene(SceneNames.Authentication);
      return null; // Важно! Возвращаем null, чтобы вызов `GetAccessToken()` понимал, что токена нет
    }

    return _accessToken;
  }

  /// <summary>
  /// Сохраяняет токены.
  /// </summary>
  public static void SaveTokens(string accessToken, string refreshToken)
  {
    // Сохраняем access-токен в статике и в SecureStorage
    _accessToken = accessToken;
    SecureStorage.SaveData(APIConstants.StorageKeys.AccessToken, accessToken);

    // Сохраняем refresh-токен в SecureStorage и сразу удаляем из памяти
    SecureStorage.SaveData(APIConstants.StorageKeys.RefreshToken, refreshToken);
  }

  public static void ClearTokensAndLogout()
  {
    _accessToken = null;
    SecureStorage.DeleteData(APIConstants.StorageKeys.AccessToken);
    SecureStorage.DeleteData(APIConstants.StorageKeys.RefreshToken);
    // Очистка всех объектов в DontDestroyOnLoad
    ClearDontDestroyOnLoadObjects();
    SceneManager.LoadScene(SceneNames.Authentication); // т.к. без токенов ничего не сделаем
  }

  private static void ClearDontDestroyOnLoadObjects()
  {
    // Найдем все объекты, помеченные DontDestroyOnLoad, и уничтожим их
    GameObject[] dontDestroyOnLoadObjects = GameObject.FindObjectsOfType<GameObject>();

    foreach (var obj in dontDestroyOnLoadObjects)
    {
      // Проверяем, что объект находится в DontDestroyOnLoad
      if (obj != null && obj.transform.parent == null)
      {
        UnityEngine.Object.Destroy(obj);
        Debug.Log($"Объект {obj.name} удалён.");
      }
    }
  }

  public static async Task LogoutAsync()
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.LogWarning("Нет интернета");
      ClearTokensAndLogout();
      Debug.LogWarning("Выход выполнен успешно без уведомления сервера");
      return;
    }

    string token = GetAccessToken();

    if (string.IsNullOrEmpty(token))
    {
      Debug.LogError("Ошибка: нет токена для выхода");
      ClearTokensAndLogout();
      Debug.LogWarning("Выход выполнен успешно без уведомления сервера");
      return; // Прерываем выполнение метода
    }

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.LogoutUrl, "POST"))
    {
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Authorization", $"Bearer {token}");  // т.к. защищённое API

      // Ожидаем завершения запроса асинхронно
      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();  // Освобождаем поток для других задач
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        Debug.Log("Выход выполнен успешно");
      }
      else
      {
        Debug.LogError($"Ошибка: {request.error}"); // Логирование ошибки
        Debug.LogWarning("Выход выполнен успешно с ошибкой на сервере");
      }

      // Этот код будет выполнен в любом случае, независимо от результата запроса
      ClearTokensAndLogout();
    }
  }
}
