using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITutorial : MonoBehaviour
{
  [SerializeField] UIManager _uiManager;

  [SerializeField] private GameObject _tutorialMenu;

  [SerializeField] private TextMeshProUGUI _tutorialText;
  [SerializeField] private Image _tutorialImage;
  [SerializeField] private Button _nextPageButton;
  [SerializeField] private Button _prevPageButton;
  [SerializeField] private Button _openTutorialButton;
  [SerializeField] private Button _closeTutorialButton;

  [SerializeField] private GameTutorial _gameTutorial;

  private List<TutorialPageData> _pages;
  private int _currentIndex = 0;

  private void Start()
  {
    _nextPageButton.onClick.AddListener(OnNextPage);
    _prevPageButton.onClick.AddListener(OnPrevPage);
    _openTutorialButton.onClick.AddListener(OpenTutorial);
    _closeTutorialButton.onClick.AddListener(CloseTutorial);

    _pages = _gameTutorial.Pages;

    if (_pages != null && _pages.Count > 0)
    {
      _currentIndex = 0;
      ShowPage();
    }
    else
    {
      _tutorialText.text = "Информации по игре нет.";
      _nextPageButton.interactable = false;
      _prevPageButton.interactable = false;
    }
  }

  private void Update()
  {
    int countGame = GameConfigManager.GetGameConfig(SelectedGameData.SelectedGameId).CountGame;
    if (countGame == 0)
      OpenTutorial();
    enabled = false;
  }

  public void OpenTutorial()
  {
    _uiManager.SetPause(true);
    _tutorialMenu.SetActive(true);
    _currentIndex = 0;
    ShowPage();
  }

  private void CloseTutorial()
  {
    _uiManager.SetPause(false);
    _tutorialMenu.SetActive(false);
  }

  private void ShowPage()
  {
    var page = _pages[_currentIndex];
    _tutorialText.text = page.Description;
    _tutorialImage.sprite = page.Image;

    _prevPageButton.interactable = _currentIndex > 0;
    _nextPageButton.interactable = _currentIndex < _pages.Count - 1;
  }

  private void OnNextPage()
  {
    if (_currentIndex < _pages.Count - 1)
    {
      _currentIndex++;
      ShowPage();
    }
  }

  private void OnPrevPage()
  {
    if (_currentIndex > 0)
    {
      _currentIndex--;
      ShowPage();
    }
  }
}
