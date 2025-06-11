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
  /// ��������� ����������� ������ ������������.
  /// </summary>
  public static async Task RegisterAsync(string email, string password, TMP_Text statusText)
  {
    await SendAuthRequestAsync(APIConstants.ApiEndpoints.RegisterUrl, email, password, statusText);
  }

  /// <summary>
  /// ��������� ���� ������������.
  /// </summary>
  public static async Task LoginAsync(string email, string password, TMP_Text statusText)
  {
    await SendAuthRequestAsync(APIConstants.ApiEndpoints.LoginUrl, email, password, statusText);
  }

  private static async Task SendAuthRequestAsync(string url, string email, string password, TMP_Text statusText)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      statusText.text = "��� ���������";
      return;
    }

    UserCredentialsRequest requestData = new UserCredentialsRequest { email = email, password = password };
    string jsonData = JsonConvert.SerializeObject(requestData);
    byte[] postData = Encoding.UTF8.GetBytes(jsonData);

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
      request.uploadHandler = new UploadHandlerRaw(postData);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json");

      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);

        if (!string.IsNullOrEmpty(response.accessToken) && !string.IsNullOrEmpty(response.refreshToken))
        {
          SaveTokens(response.accessToken, response.refreshToken);
          statusText.text = "�������� ����! \n�������������� ������...";

          if (response.playerData != null)
          {
            await PlayerDataManager.SavePlayerDataAsync(response.playerData, false);
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
          statusText.text = "������: ������������ ������ � �������";
        }
      }
      else
      {
        HandleAuthErrorResponse(request, statusText);
      }
    }
  }

  private static void HandleAuthErrorResponse(UnityWebRequest request, TMP_Text statusText)
  {
    if (request.responseCode == 0)
    {
      statusText.text = "������: ������ ����������";
      Debug.LogError("������ �����������: ������ �� ��������");
      return;
    }

    string errorMessage = request.downloadHandler.text;
    string parsedMessage = ErrorResponse.ParseErrorMessage(errorMessage);

    if (request.responseCode == 400)
    {
      statusText.text = $"������: {parsedMessage}";
    }
    else if (request.responseCode == 500)
    {
      statusText.text = "������ �������, ���������� �����.";
    }
    else
    {
      statusText.text = $"����������� ������ �������";
      Debug.LogError($"������: {parsedMessage}");
    }

    Debug.LogError($"������ ����������� ({request.responseCode}): {parsedMessage}");
  }

  public static string GetAccessToken()
  {
    if (_accessToken == null)
    {
      _accessToken = SecureStorage.GetData(APIConstants.StorageKeys.AccessToken);
    }

    if (string.IsNullOrEmpty(_accessToken))
    {
      Debug.LogWarning("Access-����� �����������. �������������� �� �����������.");
      SceneManager.LoadScene(SceneNames.Authentication);
      return null;
    }

    return _accessToken;
  }

  public static void SaveTokens(string accessToken, string refreshToken)
  {
    _accessToken = accessToken;
    SecureStorage.SaveData(APIConstants.StorageKeys.AccessToken, accessToken);

    SecureStorage.SaveData(APIConstants.StorageKeys.RefreshToken, refreshToken);
  }

  public static void ClearTokensAndLogout()
  {
    _accessToken = null;
    SecureStorage.DeleteData(APIConstants.StorageKeys.AccessToken);
    SecureStorage.DeleteData(APIConstants.StorageKeys.RefreshToken);
    ClearDontDestroyOnLoadObjects();
    SceneManager.LoadScene(SceneNames.Authentication);
  }

  private static void ClearDontDestroyOnLoadObjects()
  {
    GameObject[] dontDestroyOnLoadObjects = GameObject.FindObjectsOfType<GameObject>();

    foreach (var obj in dontDestroyOnLoadObjects)
    {
      if (obj != null && obj.transform.parent == null)
      {
        UnityEngine.Object.Destroy(obj);
        Debug.Log($"������ {obj.name} �����.");
      }
    }
  }

  public static async Task LogoutAsync()
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.LogWarning("��� ���������");
      ClearTokensAndLogout();
      Debug.LogWarning("����� �������� ������� ��� ����������� �������");
      return;
    }

    string token = GetAccessToken();

    if (string.IsNullOrEmpty(token))
    {
      Debug.LogError("������: ��� ������ ��� ������");
      ClearTokensAndLogout();
      Debug.LogWarning("����� �������� ������� ��� ����������� �������");
      return;
    }

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.LogoutUrl, "POST"))
    {
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Authorization", $"Bearer {token}");

      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        Debug.Log("����� �������� �������");
      }
      else
      {
        Debug.LogError($"������: {request.error}");
        Debug.LogWarning("����� �������� ������� � ������� �� �������");
      }

      ClearTokensAndLogout();
    }
  }

  public static async Task SendRequestPasswordResetAsync(string email, TMP_Text statusText)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      statusText.text = "��� ���������";
      return;
    }

    var requestData = new { email = email };
    string jsonData = JsonConvert.SerializeObject(requestData);
    byte[] postData = Encoding.UTF8.GetBytes(jsonData);

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.PasswordResetRequestUrl, "POST"))
    {
      request.uploadHandler = new UploadHandlerRaw(postData);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json");

      var operation = request.SendWebRequest();
      while (!operation.isDone)
      {
        await Task.Yield();
      }

      if (request.result == UnityWebRequest.Result.Success)
      {
        statusText.text = "������ ��� ������ ������ ���������� �� email.";
      }
      else
      {
        statusText.text = "������ �������, ���������� �����.";
        Debug.LogError($"������ ������� ������ ������: {request.error}");
      }
    }
  }
}
