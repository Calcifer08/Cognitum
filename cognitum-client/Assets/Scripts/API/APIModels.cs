using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Запрос для авторизации.
/// </summary>
public class UserCredentialsRequest
{
  public string email;
  public string password;
}

/// <summary>
/// Ответ с токенами после авторизации.
/// </summary>
public class TokensResponse
{
  public string accessToken;
  public string refreshToken;
}

public class AuthResponse
{
  public string accessToken;
  public string refreshToken;
  public PlayerData playerData;
  public Dictionary<string, GameConfig> gameConfigData;
  public GameStatistics gameStatistics;
}

/// <summary>
/// Ошибка, возвращаемая сервером.
/// </summary>
public class ErrorResponse
{
  public string message;

  public static string ParseErrorMessage(string json)
  {
    try
    {
      ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(json);
      return errorResponse?.message ?? "Неизвестная ошибка на сервере";
    }
    catch
    {
      return "Ошибка парсинга ответа сервера";
    }
  }
}

/// <summary>
/// Запрос на обновление токена.
/// </summary>
public class RefreshTokenRequest
{
  public string refreshToken;
}
