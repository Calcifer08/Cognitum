using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public static class TokenRefreshManager
{
  public static async Task RefreshTokenAsync()
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.LogWarning("Нет интернета");
      return;
    }

    string refreshToken = SecureStorage.GetData(APIConstants.StorageKeys.RefreshToken);

    if (string.IsNullOrEmpty(refreshToken))
    {
      Debug.LogWarning("Нет рефреш-токена");
      AuthManager.ClearTokensAndLogout();
      return;
    }

    RefreshTokenRequest requestData = new RefreshTokenRequest { refreshToken = refreshToken };
    string jsonData = JsonConvert.SerializeObject(requestData);
    byte[] postData = Encoding.UTF8.GetBytes(jsonData);

    using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.RefreshUrl, "POST"))
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
        try
        {
          TokensResponse tokensResponse = JsonConvert.DeserializeObject<TokensResponse>(request.downloadHandler.text);

          if (tokensResponse != null &&
              !string.IsNullOrEmpty(tokensResponse.accessToken) &&
              !string.IsNullOrEmpty(tokensResponse.refreshToken))
          {
            AuthManager.SaveTokens(tokensResponse.accessToken, tokensResponse.refreshToken);
          }
          else
          {
            Debug.LogError($"Ошибка: некорректные данные с сервера. Ответ: {request.downloadHandler.text}");
          }
        }
        catch (Exception ex)
        {
          Debug.LogError($"Ошибка парсинга JSON: {ex.Message}. Ответ: {request.downloadHandler.text}");
        }
      }
      else
      {
        HandleErrorResponse(request);
      }
    }
  }

  private static void HandleErrorResponse(UnityWebRequest request)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("Ошибка авторизации: сервер не отвечает");
      return;
    }

    string errorMessage = request.downloadHandler.text;
    string parsedMessage = ErrorResponse.ParseErrorMessage(errorMessage);

    if (request.responseCode == 403)
    {
      Debug.LogError($"Ошибка 403: {parsedMessage}. Перенаправляем на авторизацию.");
      AuthManager.ClearTokensAndLogout();
      SceneManager.LoadScene(SceneNames.Authentication);
    }
    else if (request.responseCode >= 500 && request.responseCode < 600)
    {
      Debug.LogError($"Ошибка сервера ({request.responseCode}): {parsedMessage}. Попробуйте позже.");
    }
    else
    {
      Debug.LogError($"Ошибка обновления токена ({request.responseCode}): {parsedMessage}");
    }
  }
}
