using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
  [SerializeField] private UITimer _uiTimer;
  [SerializeField] private TimerManager _timerManager;

  [SerializeField] private UIProgress _uiProgress;

  [SerializeField] private UITutorial _uiTutorial;


  public void SetPause(bool isPause)
  {
    _timerManager.SetPause(isPause);
  }


  public void UpdateTimerSession(int minutes, int seconds)
  {
    _uiTimer.UpdateTimerSession(minutes, seconds);
  }

  public void UpdateTimerQuestion(float normalizedTime)
  {
    _uiTimer.UpdateTimerQuestion(normalizedTime);
  }
  


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
