
public static class APIConstants
{
  public static class StorageKeys
  {
    public const string AccessToken = "access_token";
    public const string RefreshToken = "refresh_token";
  }

  public static class ApiEndpoints
  {
    private const string BaseUrl = "https://magical-deer-flying.ngrok-free.app/api";
    public const string RegisterUrl = BaseUrl + "/auth/register";
    public const string LoginUrl = BaseUrl + "/auth/login";
    public const string LogoutUrl = BaseUrl + "/auth/logout";
    public const string RefreshUrl = BaseUrl + "/auth/refresh-token";
    public const string SaveLogsUrl = BaseUrl + "/logs/save";
    public const string SavePlayerDataUrl = BaseUrl + "/player/update";
    public const string SaveGameConfigUrl = BaseUrl + "/game-config/update";
    public const string SaveGameStatisticksUrl = BaseUrl + "/statistics/update";
  }
}
