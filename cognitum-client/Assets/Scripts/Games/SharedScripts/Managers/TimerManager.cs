using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
  [SerializeField] private UIManager _uiManager;

  public event Action OnGameTimeOver;
  [SerializeField] private float _timeSession = 60f;
  private bool _isTimeOver = false;

  public event Action<int, string> OnPauseStarted;
  public event Action<int, float> OnPauseEnded;
  private int _numberPause = 0;
  private float pauseStartTime = 0f;  // ¬рем€ начала паузы

  public event Action OnEndTimeAnswerPhase;
  private bool _isAnswerTimerRunning = false;
  private float _timeQuestionLimit = 0f;
  private float _currentTimeQuestion;

  private void Start()
  {
    UpdateTimerSession();
  }

  private void Update()
  {
    // если пауза или врем€ игры вышло
    if (Time.timeScale == 0f || _isTimeOver) return;

    if (_timeSession > 0f)
    {
      _timeSession -= Time.deltaTime;
      UpdateTimerSession();
    }
    else
    {
      _isTimeOver = true;
      OnGameTimeOver?.Invoke();
    }

    if (_isAnswerTimerRunning)
    {
      // если есть врем€ на вопрос
      if (_currentTimeQuestion > 0f)
      {
        _currentTimeQuestion -= Time.deltaTime;
        UpdateTimerQuestion();
      }
      else
      {
        _isAnswerTimerRunning = false;
        OnEndTimeAnswerPhase?.Invoke();
      }
    }
  }

  public void SetTimerSession(float time)
  {
    _timeSession = time;
  }

  public void UpdateTimerSession()
  {
    if (_timeSession < 0)
    {
      _timeSession = 0;
    }

    int totalSeconds = Mathf.CeilToInt(_timeSession);
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;
    _uiManager.UpdateTimerSession(minutes, seconds);
  }

  public void SetTimerQuestion(float time)
  {
    _timeQuestionLimit = time;
    _currentTimeQuestion = time;
    _isAnswerTimerRunning = true;
  }

  public void UpdateTimerQuestion()
  {
    _uiManager.UpdateTimerQuestion(_currentTimeQuestion / _timeQuestionLimit);
  }

  public void CancelAnswerTimer()
  {
    _isAnswerTimerRunning = false;
  }


  public void SetPause(bool isPause)
  {
    if (isPause)
    {
      Time.timeScale = 0f;
      pauseStartTime = Time.realtimeSinceStartup;
      _numberPause++;
      OnPauseStarted?.Invoke(_numberPause, "The Game is Paused");
      //_logSessionManager.AddPauseStartToLog(_numberPause, "The Game is Paused");
    }
    else
    {
      Time.timeScale = 1f;
      float pauseDuration = Time.realtimeSinceStartup - pauseStartTime;
      OnPauseEnded?.Invoke(_numberPause, pauseDuration);
      //_logSessionManager.AddPauseEndToLog(_numberPause, pauseDuration);
    }
  }

  // ¬ызываетс€, когда приложение сворачиваетс€ или восстанавливаетс€
  void OnApplicationPause(bool isPaused)
  {
    if (isPaused)
    {
      // ѕриложение свернуто Ч сохран€ем врем€
      pauseStartTime = Time.realtimeSinceStartup;
      _numberPause++;
      OnPauseStarted?.Invoke(_numberPause, "The app is minimized");
    }
    else
    {
      // ѕриложение восстановлено Ч вычисл€ем длительность паузы
      float pauseDuration = Time.realtimeSinceStartup - pauseStartTime;
      OnPauseEnded?.Invoke(_numberPause, pauseDuration);
    }
  }
}
