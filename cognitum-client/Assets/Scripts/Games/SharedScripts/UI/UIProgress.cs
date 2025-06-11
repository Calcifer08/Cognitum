using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgress : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI _scoreText;
  [SerializeField] private TextMeshProUGUI _mistakeText;
  [SerializeField] private TextMeshProUGUI _levelText;
  [SerializeField] private TextMeshProUGUI _correctText;

  [SerializeField] private GameObject _answerResultImages;
  private Image _resultImageLeft;
  private Image _resultImageRight;

  private Coroutine _currentResultCoroutine;

  private void Awake()
  {
    _resultImageLeft = _answerResultImages.transform.Find("Left").GetComponent<Image>();
    _resultImageRight = _answerResultImages.transform.Find("Right").GetComponent<Image>();

    _scoreText.text = "Очки: 0";
  }

  public void UpdateScore(int score)
  {
    string currentScore = $"Очки: {score}";
    _scoreText.text = currentScore;
  }

  public void UpdateLevel(int level)
  {
    string currentLevel = $"Уровень: {level}";
    _levelText.text = currentLevel;
  }

  public void UpdateCorrect(int correct, int maxCorrect)
  {
    string currentCorrect = $"Верно: {correct}/{maxCorrect}";
    _correctText.text = currentCorrect;
  }

  public void UpdateMistake(int mistake, int maxMistake)
  {
    string currentMistake = $"Ошибки {mistake}/{maxMistake}";
    _mistakeText.text = currentMistake;
  }

  public IEnumerator DisplayResultCoroutine(bool isGreen, float resultDisplayTime)
  {
    if (_currentResultCoroutine != null)
    {
      StopCoroutine(_currentResultCoroutine);
    }

    Color resultColor = isGreen ? new Color32(46, 157, 124, 255) : new Color32(242, 37, 50, 255);

    _currentResultCoroutine = StartCoroutine(AnimateResultCoroutine(resultColor, resultDisplayTime));

    yield return _currentResultCoroutine;
  }

  private IEnumerator AnimateResultCoroutine(Color resultColor, float resultDisplayTime)
  {
    _resultImageLeft.color = resultColor;
    _resultImageRight.color = resultColor;
    _answerResultImages.SetActive(true);

    yield return new WaitForSeconds(resultDisplayTime);

    _answerResultImages.SetActive(false);
    _resultImageLeft.color = Color.white;
    _resultImageRight.color = Color.white;
  }
}
