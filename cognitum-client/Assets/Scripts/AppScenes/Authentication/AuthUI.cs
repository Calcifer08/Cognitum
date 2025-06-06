using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthUI : MonoBehaviour
{
  [SerializeField] private TMP_Text _headText;

  [SerializeField] private TMP_InputField _emailInput;

  [SerializeField] private TMP_InputField _passwordInput;
  [SerializeField] private Button _showPasswordButton;
  private bool _isPasswordVisible = false;

  [SerializeField] private TMP_InputField _confirmPasswordInput;
  [SerializeField] private Button _showConfirmPasswordButton;
  private bool _isConfirmPasswordVisible = false;

  [SerializeField] private Sprite _openEyeSprite;
  [SerializeField] private Sprite _closedEyeSprite;

  [SerializeField] private Button _authButton;

  private TMP_Text _authButtonText;

  [SerializeField] private TMP_Text _switchAuthText;

  [SerializeField] private Image _statusImage;
  private TMP_Text _statusText;

  private bool _isProcessing = false;

  private void Start()
  {
    _statusImage.gameObject.SetActive(false);
    _statusText = _statusImage.GetComponentInChildren<TMP_Text>();

    _authButton.onClick.AddListener(OnLoginClicked);
    _authButtonText = _authButton.GetComponentInChildren<TMP_Text>();

    _showPasswordButton.onClick.AddListener(() => TogglePasswordVisibility(_passwordInput));
    _showConfirmPasswordButton.onClick.AddListener(() => TogglePasswordVisibility(_confirmPasswordInput));
  }

  public void SwitchToLogin()
  {
    _statusImage.gameObject.SetActive(false);
    _headText.text = "Вход";
    _switchAuthText.text = "Нет аккаунта? <link=register><color=blue><u>Зарегистрироваться!</u></color></link>";
    _authButtonText.text = "Войти";
    _confirmPasswordInput.gameObject.SetActive(false);
    _authButton.onClick.RemoveAllListeners();
    _authButton.onClick.AddListener(OnLoginClicked);
  }

  public void SwitchToRegis()
  {
    _statusImage.gameObject.SetActive(false);
    _headText.text = "Регистрация";
    _switchAuthText.text = "Уже есть аккаунт? <link=login><color=blue><u>Войти!</u></color></link>";
    _authButtonText.text = "Зарегистрироваться";
    _confirmPasswordInput.gameObject.SetActive(true);
    _authButton.onClick.RemoveAllListeners();
    _authButton.onClick.AddListener(OnRegisterClicked);
  }

  /// <summary>
  /// Обработчик кнопки входа.
  /// </summary>
  private async void OnLoginClicked()
  {
    if (!_isProcessing)
    {
      _statusImage.gameObject.SetActive(false);

      string email = _emailInput.text.Trim();
      string password = _passwordInput.text.Trim();

      if (!ValidateInput(email, password, password))
      {
        _statusImage.gameObject.SetActive(true);
        return;
      }

      _isProcessing = true;

      await AuthManager.LoginAsync(email, password, _statusText);

      _isProcessing = false;
    }
  }

  /// <summary>
  /// Обработчик кнопки регистрации.
  /// </summary>
  private async void OnRegisterClicked()
  {
    if (!_isProcessing)
    {
      _statusImage.gameObject.SetActive(false);

      string email = _emailInput.text.Trim();
      string password = _passwordInput.text.Trim();
      string confirmPassword = _confirmPasswordInput.text.Trim();

      if (!ValidateInput(email, password, confirmPassword))
      {
        _statusImage.gameObject.SetActive(true);
        return;
      }

      _isProcessing = true;

      await AuthManager.RegisterAsync(email, password, _statusText);

      _isProcessing = false;
    }
  }

  /// <summary>
  /// Валидация email и пароля.
  /// </summary>
  private bool ValidateInput(string email, string password, string confirmPassword)
  {
    if (!IsValidEmail(email))
    {
      _statusText.text = "Некорректный email!";
      return false;
    }

    if (!IsValidPassword(password))
    {
      _statusText.text = "Пароль должен быть не менее 6 символов!";
      return false;
    }

    if (password != confirmPassword)
    {
      _statusText.text = "Пароли не совпадают!";
      return false;
    }

    return true;
  }

  /// <summary>
  /// Регулярная проверка email.
  /// </summary>
  private bool IsValidEmail(string email)
  {
    return System.Text.RegularExpressions.Regex.IsMatch(email,
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
  }

  /// <summary>
  /// Проверка пароля (не менее 6 символов).
  /// </summary>
  private bool IsValidPassword(string password)
  {
    return password.Length >= 6;
  }

  /// <summary>
  /// Переключение видимости пароля для переданного TMP_InputField.
  /// </summary>
  private void TogglePasswordVisibility(TMP_InputField inputField)
  {
    if (inputField == _passwordInput)
    {
      _isPasswordVisible = !_isPasswordVisible;
      inputField.contentType = _isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
      _showPasswordButton.image.sprite = _isPasswordVisible ? _openEyeSprite : _closedEyeSprite;
    }
    else if (inputField == _confirmPasswordInput)
    {
      _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
      inputField.contentType = _isConfirmPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
      _showConfirmPasswordButton.image.sprite = _isConfirmPasswordVisible ? _openEyeSprite : _closedEyeSprite;
    }

    // Применяем изменения
    inputField.ForceLabelUpdate();
  }
}
