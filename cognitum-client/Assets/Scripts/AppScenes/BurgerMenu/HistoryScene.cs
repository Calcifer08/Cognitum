using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HistoryScene : MonoBehaviour
{
  private static HistoryScene _instance;

  private Stack<string> _sceneHistory = new Stack<string>();

  private float _lastBackPressedTime = -1f; // Время последнего нажатия Escape
  private float _doublePressPeriod = 1.5f; // интервал между нажатиями

  void Awake()
  {
    if (_instance == null)
    {
      // если пустой, то этот станет оригиналом
      _instance = this;
      // запрещаем удаления при загрузке другой сцены
      DontDestroyOnLoad(gameObject);

      // Подписка на событие загрузки сцены
      SceneManager.sceneLoaded += OnSceneLoaded;

      string nameScene = SceneManager.GetActiveScene().name;
      if (nameScene == SceneNames.Profile)
        _sceneHistory.Push(nameScene);
    }
    else
    {
      // если оригинал есть, то этот удалим
      Destroy(gameObject);
    }
  }

  // Метод вызывается автоматически при каждой полной загрузке новой сцены
  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    // Игнорируем аддитивную загрузку
    if (mode == LoadSceneMode.Additive)
      return;

    string sceneName = scene.name;

    // Пропускаем игровые и итоговые сцены
    if (GameDataManager.IsGameScene(sceneName) || sceneName == SceneNames.EndGame)
      return;

    // Запоминаем сцену, если это не дубль
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
      // Если стек пуст или одна сцена — ждём повторное нажатие
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