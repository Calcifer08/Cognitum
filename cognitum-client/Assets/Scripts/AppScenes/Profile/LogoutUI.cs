using UnityEngine;
using UnityEngine.UI;

public class LogoutUI : MonoBehaviour
{
  [SerializeField] private Button _logoutButton;

  private bool _isProcessing = false;

  private void Awake()
  {
    _logoutButton.onClick.AddListener(Logout);
  }

  private async void Logout()
  {
    if (!_isProcessing)
    {
      _isProcessing = true;

      await AuthManager.LogoutAsync();

      _isProcessing = false;
    }
  }
}
