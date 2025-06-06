using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// ������ ��� �����������.
/// </summary>
public class UserCredentialsRequest
{
  public string email;
  public string password;
}

/// <summary>
/// ����� � �������� ����� �����������.
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
/// ������, ������������ ��������.
/// </summary>
public class ErrorResponse
{
  public string message;

  public static string ParseErrorMessage(string json)
  {
    try
    {
      ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(json);
      return errorResponse?.message ?? "����������� ������ �� �������";
    }
    catch
    {
      return "������ �������� ������ �������";
    }
  }
}

/// <summary>
/// ������ �� ���������� ������.
/// </summary>
public class RefreshTokenRequest
{
  public string refreshToken;
}
