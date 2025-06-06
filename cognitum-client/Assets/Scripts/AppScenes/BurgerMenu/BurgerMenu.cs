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
    SceneManager.sceneLoaded += OnSceneLoaded; // подписались на событие загрузки сцены
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded; // отписались
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
    _canvasGroup.alpha = 0; // невидимый
    _canvasGroup.interactable = false; // некликабельный
    _canvasGroup.blocksRaycasts = false; // пропускает лучи
  }

  private void ShowBurgerMenu()
  {
    Time.timeScale = 1.0f; // на всякий случай, ибо в игре можем поставить на паузу и как-то попасть в меню
    _canvasGroup.alpha = 1f; // видимый
    _canvasGroup.interactable = true; // кликабельный
    _canvasGroup.blocksRaycasts = true; // не пропускает лучи
  }

  private void Start()
  {
    // значение должно быть выше, чем у других Canvas, иначе перекроют друг друга
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
    
    // ибо смысл загружать текущую сцену
    if (SceneManager.GetActiveScene().name != nameScene)
    {
      SceneManager.LoadScene(nameScene);
    }
  }
}
