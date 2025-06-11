using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AboutGameViewer : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI _aboutGameText;
  [SerializeField] private Button _nextButton;
  [SerializeField] private Button _prevButton;

  [SerializeField] private AllAboutGame _allAboutGame;

  private List<string> _currentPages;
  private int _currentIndex = 0;

  private void Start()
  {
    _nextButton.onClick.AddListener(OnNext);
    _prevButton.onClick.AddListener(OnPrev);
  }

  /// <summary>
  /// Загружает туториал для выбранной игры по её GameId
  /// </summary>
  public void Init(string gameId)
  {
    AboutGame aboutGame = _allAboutGame.AboutGames.Find(t => t.GameId == gameId);

    if (aboutGame != null && aboutGame.AboutPages != null && aboutGame.AboutPages.Count > 0)
    {
      _currentPages = aboutGame.AboutPages;
      _currentIndex = 0;
      ShowPage();
    }
    else
    {
      _aboutGameText.text = "Информации по игре нет.";
      _nextButton.interactable = false;
      _prevButton.interactable = false;
    }
  }

  private void ShowPage()
  {
    _aboutGameText.text = _currentPages[_currentIndex];

    _prevButton.interactable = _currentIndex > 0;
    _nextButton.interactable = _currentIndex < _currentPages.Count - 1;
  }

  public void OnNext()
  {
    if (_currentIndex < _currentPages.Count - 1)
    {
      _currentIndex++;
      ShowPage();
    }
  }

  public void OnPrev()
  {
    if (_currentIndex > 0)
    {
      _currentIndex--;
      ShowPage();
    }
  }
}
