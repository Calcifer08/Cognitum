using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLogin : MonoBehaviour
{
  async void Start()
  {
    // !!! ������ ��� ������������� ���������� ������������
    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

    string accessToken = SecureStorage.GetData(APIConstants.StorageKeys.AccessToken);

    if (!string.IsNullOrEmpty(accessToken))
    {
      // �.�. GameStatisticsManager ������� �� ����
      while (!GameDataManager.IsInitialized)
      {
        await Task.Yield();
      }

      await Task.WhenAll(
        PlayerDataManager.InitializeAsync(),
        GameConfigManager.InitializeAsync(),
        AppSettingsManager.InitializeAsync(),
        GameSessionManager.InitializeAsync(),
        GameStatisticsManager.InitializeAsync() // ����������� ������� ������ ���� ������ GameDataInitializer
      );

      _ = DataSyncManager.InitializeAsync();

      SceneManager.LoadScene(SceneNames.MenuCategoryGame);
    }
    else
    {
      // �.�. GameStatisticsManager ������� �� ����
      while (!GameDataManager.IsInitialized)
      {
        await Task.Yield();
      }

      GameStatisticsManager.InitializeCategoryList(); // � ����� ������ ���� ����������������
      SceneManager.LoadScene(SceneNames.Authentication);
    }
  }
}
