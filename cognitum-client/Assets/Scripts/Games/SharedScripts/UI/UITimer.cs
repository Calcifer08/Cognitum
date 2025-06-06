using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI _timerText;
  [SerializeField] private Image _timeQuestionImage;
  [SerializeField] private Image _timeQuestionImageBack;
  private RectTransform _fillRect;
  private Vector2 _originalSize;

  private float _lastAlpha = 0f;

  private void Awake()
  {
    _fillRect = _timeQuestionImage.gameObject.GetComponent<RectTransform>();
    _originalSize = _fillRect.sizeDelta;

    _timeQuestionImage.canvasRenderer.SetAlpha(0f);
    _timeQuestionImageBack.canvasRenderer.SetAlpha(0f);
  }

  public void UpdateTimerSession(int minutes, int seconds)
  {
    string currentTime = string.Format("{0:0}:{1:00}", minutes, seconds);
    _timerText.text = currentTime;
  }

  public void UpdateTimerQuestion(float normalizedTime)
  {
    if (normalizedTime <= 0f)
    {
      if (_lastAlpha != 0f)
      {
        _timeQuestionImage.canvasRenderer.SetAlpha(0f);
        _timeQuestionImageBack.canvasRenderer.SetAlpha(0f);
        _lastAlpha = 0f;
        return;
      }
    }
    else
    {
      if (_lastAlpha != 1f)
      {
        _timeQuestionImage.canvasRenderer.SetAlpha(1f);
        _timeQuestionImageBack.canvasRenderer.SetAlpha(1f);
        _lastAlpha = 1f;
      }
    }

    float newWidth = Mathf.Lerp(0f, _originalSize.x, normalizedTime);
    _fillRect.sizeDelta = new Vector2(newWidth, _originalSize.y);
  }
}
  //public void UpdateTimerQuestion(float normalizedTime)
  //{
  //  _timeQuestionImage.fillAmount = normalizedTime;

  //  if (normalizedTime <= 0f)
  //  {
  //    if (_lastAlpha != 0f)
  //    {
  //      _timeQuestionImage.canvasRenderer.SetAlpha(0f);
  //      _timeQuestionImageBack.canvasRenderer.SetAlpha(0f);
  //      _lastAlpha = 0f;
  //    }
  //  }
  //  else
  //  {
  //    if (_lastAlpha != 1f)
  //    {
  //      _timeQuestionImage.canvasRenderer.SetAlpha(1f);
  //      _timeQuestionImageBack.canvasRenderer.SetAlpha(1f);
  //      _lastAlpha = 1f;
  //    }

  //    if (normalizedTime > 0.5f)
  //    {
  //      _timeQuestionImage.color = Color.Lerp(Color.green, Color.yellow, 1 - (normalizedTime - 0.5f) * 2);
  //    }
  //    else
  //    {
  //      _timeQuestionImage.color = Color.Lerp(Color.yellow, Color.red, 1 - normalizedTime * 2);
  //    }
  //  }
  //}
