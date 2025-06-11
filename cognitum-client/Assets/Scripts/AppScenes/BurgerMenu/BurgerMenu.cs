using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BurgerMenu : MonoBehaviour
{
  [SerializeField] private GameObject _burgerMenuObject;
  [SerializeField] private Button _buttonOpenMenu;
  [SerializeField] private Button _buttonCloseMenu;
  [SerializeField] private Button _buttonCloseMenuAndBackground;
  [SerializeField] private Button _profile;
  [SerializeField] private Button _games;
  [SerializeField] private Button _statistics;
  [SerializeField] private Button _settings;

  private CanvasGroup _canvasGroup;

  private static BurgerMenu _instance;

  private void Awake()
  {
    if (_instance == null)
    {
      _instance = this;

      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }

    _canvasGroup = GetComponent<CanvasGroup>();
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    string sceneName = SceneManager.GetActiveScene().name;
    if (GameDataManager.IsGameScene(sceneName))
    {
      HideBurgerMenu();
    }
    else
    {
      ShowBurgerMenu();
    }
  }

  private void HideBurgerMenu()
  {
    _canvasGroup.alpha = 0;
    _canvasGroup.interactable = false;
    _canvasGroup.blocksRaycasts = false;
  }

  private void ShowBurgerMenu()
  {
    Time.timeScale = 1.0f;
    _canvasGroup.alpha = 1f;
    _canvasGroup.interactable = true;
    _canvasGroup.blocksRaycasts = true;
  }

  private void Start()
  {
    gameObject.GetComponent<Canvas>().sortingOrder = 10;
    _buttonOpenMenu.onClick.AddListener(OpenMenu);
    _buttonCloseMenu.onClick.AddListener(CloseMenu);
    _buttonCloseMenuAndBackground.onClick.AddListener(CloseMenu);
    _profile.onClick.AddListener(() => LoadScene(SceneNames.Profile));
    _games.onClick.AddListener(() => LoadScene(SceneNames.MenuCategoryGame));
    _statistics.onClick.AddListener(() => LoadScene(SceneNames.Statistics));
    _settings.onClick.AddListener(() => LoadScene(SceneNames.Settings));
  }

  private void OpenMenu()
  {
    _burgerMenuObject.SetActive(true);
    _buttonOpenMenu.gameObject.SetActive(false);
  }

  private void CloseMenu()
  {
    _burgerMenuObject.SetActive(false);
    _buttonOpenMenu.gameObject.SetActive(true);
  }

  private void LoadScene(string nameScene)
  {
    CloseMenu();
    
    if (SceneManager.GetActiveScene().name != nameScene)
    {
      SceneManager.LoadScene(nameScene);
    }
  }
}
