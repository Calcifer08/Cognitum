using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HistoryScene : MonoBehaviour
{
  private static HistoryScene _instance;

  private Stack<string> _sceneHistory = new Stack<string>();

  private float _lastBackPressedTime = -1f;
  private float _doublePressPeriod = 1.5f;

  void Awake()
  {
    if (_instance == null)
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);

      SceneManager.sceneLoaded += OnSceneLoaded;

      string nameScene = SceneManager.GetActiveScene().name;
      if (nameScene == SceneNames.Profile)
        _sceneHistory.Push(nameScene);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    if (mode == LoadSceneMode.Additive)
      return;

    string sceneName = scene.name;

    if (GameDataManager.IsGameScene(sceneName) || sceneName == SceneNames.EndGame)
      return;

    if (_sceneHistory.Count == 0 || _sceneHistory.Peek() != sceneName)
    {
      _sceneHistory.Push(sceneName);
    }
  }

  private void Update()
  {
#if UNITY_ANDROID && !UNITY_EDITOR
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      GoBackScene();
    }
#else
    if (Input.GetMouseButtonDown(1))
    {
      GoBackScene();
    }
#endif
  }

  private void GoBackScene()
  {
    if (_sceneHistory.Count > 1)
    {
      _sceneHistory.Pop();
      SceneManager.LoadScene(_sceneHistory.Peek());
    }
    else
    {
      if (Time.time - _lastBackPressedTime < _doublePressPeriod)
      {
        Application.Quit();
      }
      else
      {
#if UNITY_ANDROID
        AndroidToast.Show("Нажмите ещё раз для выхода");
#else
        Debug.Log("Нажмите ещё раз для выхода");
#endif
        _lastBackPressedTime = Time.time;
      }
    }
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }
}