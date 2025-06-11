using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLogin : MonoBehaviour
{
  async void Start()
  {
    // !!! убрать при использовании нормальных сертификатов
    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

    string accessToken = SecureStorage.GetData(APIConstants.StorageKeys.AccessToken);

    if (!string.IsNullOrEmpty(accessToken))
    {
      while (!GameDataManager.IsInitialized)
      {
        await Task.Yield();
      }

      await Task.WhenAll(
        PlayerDataManager.InitializeAsync(),
        GameConfigManager.InitializeAsync(),
        AppSettingsManager.InitializeAsync(),
        GameSessionManager.InitializeAsync(),
        GameStatisticsManager.InitializeAsync()
      );

      _ = DataSyncManager.InitializeAsync();

      SceneManager.LoadScene(SceneNames.MenuCategoryGame);
    }
    else
    {
      while (!GameDataManager.IsInitialized)
      {
        await Task.Yield();
      }

      GameStatisticsManager.InitializeCategoryList();
      SceneManager.LoadScene(SceneNames.Authentication);
    }
  }
}
