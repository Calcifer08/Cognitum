using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AboutGameUIManager : MonoBehaviour
{
  [SerializeField] private AboutGameViewer _tutorialViewer;
  [SerializeField] private LevelSelector _levelSelector;

  [SerializeField] private Button _startButton;
  [SerializeField] private Button _backButton;

  private string _gameId;

  private void Start()
  {
    _gameId = SelectedGameData.SelectedGameId;

    _tutorialViewer.Init(_gameId);

    // Инициализируем слайдер уровня
    _levelSelector.Init(_gameId);

    // Подписка на кнопки
    _startButton.onClick.AddListener(OnStartClicked);
    _backButton.onClick.AddListener(OnBackClicked);
  }

  private void OnStartClicked()
  {
    _levelSelector.SaveSelectedLevel();
    string sceneToLoad = GameDataManager.GetSceneName(_gameId);
    SceneManager.LoadScene(sceneToLoad);
  }

  private void OnBackClicked()
  {
    SceneManager.LoadScene(SceneNames.MenuCategoryGame);
  }
}
