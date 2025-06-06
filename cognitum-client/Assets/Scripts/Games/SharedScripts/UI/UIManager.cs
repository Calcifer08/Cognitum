using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
  // для ui таймера
  [SerializeField] private UITimer _uiTimer;
  [SerializeField] private TimerManager _timerManager;

  // для ui прогресса
  [SerializeField] private UIProgress _uiProgress;

  // для ui туториала
  [SerializeField] private UITutorial _uiTutorial;


  // взаимодействие с менеджером таймера
  public void SetPause(bool isPause)
  {
    _timerManager.SetPause(isPause);
  }


  // UI таймера
  public void UpdateTimerSession(int minutes, int seconds)
  {
    _uiTimer.UpdateTimerSession(minutes, seconds);
  }

  public void UpdateTimerQuestion(float normalizedTime)
  {
    _uiTimer.UpdateTimerQuestion(normalizedTime);
  }
  


  // UI прогресса
  public void UpdateScore(int score)
  {
    _uiProgress.UpdateScore(score);
  }

  public void UpdateLevel(int level)
  {
    _uiProgress.UpdateLevel(level);
  }

  public void UpdateCorrect(int correct, int maxCorrect)
  {
    _uiProgress.UpdateCorrect(correct, maxCorrect);
  }

  public void UpdateMistake(int mistake, int maxMistake)
  {
    _uiProgress.UpdateMistake(mistake, maxMistake);
  }

  public IEnumerator DisplayResultCoroutine(bool isGreen, float resultDisplayTime)
  {
    yield return _uiProgress.DisplayResultCoroutine(isGreen, resultDisplayTime);
  }
}
