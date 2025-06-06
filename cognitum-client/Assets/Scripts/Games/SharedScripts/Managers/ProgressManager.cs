using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour
{
  [SerializeField] private UIManager _uiMananger;

  public int Score { get; private set; } = 0;
  private int _сorrectAnswerPoint = 3;
  private int _incorrectAnswerPoint = 1;
  private int _levelWinPoints = 5;
  private int _levelFailPoints = 3;

  // обновляем очки под уровень
  public void UpdatePointsForLevel(int level)
  {
    _сorrectAnswerPoint = 3 * level;
    _incorrectAnswerPoint = 1 * level;
    _levelWinPoints = 5 * level;
    _levelFailPoints = 3 * level;
  }


  public void AddPointsForCorrectAnswer()
  {
    Score += _сorrectAnswerPoint;
    UpdateScore();
  }

  public void SubtractPointsForIncorrectAnswer()
  {
    Score = Math.Max(0, Score - _incorrectAnswerPoint);
    UpdateScore();
  }

  public void AddPointsForLevelWin()
  {
    Score += _levelWinPoints;
    UpdateScore();
  }

  public void SubtractPointsForLevelFailure()
  {
    Score = Math.Max(0, Score - _levelFailPoints);
    UpdateScore();
  }


  private void UpdateScore()
  {
    _uiMananger.UpdateScore(Score);
  }

  public void UpdateMistake(int mistake, int maxMistake)
  {
    _uiMananger.UpdateMistake(mistake, maxMistake);
  }

  public void UpdateLevel(int level)
  {
    _uiMananger.UpdateLevel(level);
  }

  public void UpdateCorrect(int correct, int maxCorrect)
  {
    _uiMananger.UpdateCorrect(correct, maxCorrect);
  }

  // отображает правильность ответа (пока только цвет, но может потом и ожидаемый ответ)
  public IEnumerator DisplayResultCoroutine(bool isGreen, float resultDisplayTime)
  {
   yield return _uiMananger.DisplayResultCoroutine(isGreen, resultDisplayTime);
  }
}
