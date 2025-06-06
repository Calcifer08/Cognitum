using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPause : MonoBehaviour
{
  [SerializeField] UIManager _uiManager;

  [SerializeField] private GameObject _pauseMenu;
  [SerializeField] private Button _pauseButton;
  [SerializeField] private Button _resumeButton;
  [SerializeField] private Button _SettingsButton;
  [SerializeField] private Button _backMenuButton;

  private void Start()
  {
    _pauseButton.onClick.AddListener(ButtonPause);
    _resumeButton.onClick.AddListener(ButtonResume);
    _backMenuButton.onClick.AddListener(ButtonBackMenu);
    _SettingsButton.onClick.AddListener(ButtonOpenSettings);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      ButtonPause();
    }
  }

  private void ButtonPause()
  {
    _uiManager.SetPause(true);
    _pauseMenu.SetActive(true);
  }

  private void ButtonResume()
  {
    _pauseMenu.SetActive(false);
    _uiManager.SetPause(false);
  }

  private void ButtonBackMenu()
  {
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneNames.MenuCategoryGame);
  }

  private void ButtonOpenSettings()
  {
    SceneManager.LoadSceneAsync(SceneNames.Settings, LoadSceneMode.Additive);
  }
}
