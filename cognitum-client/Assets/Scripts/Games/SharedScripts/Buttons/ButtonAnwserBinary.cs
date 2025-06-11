using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnwserBinary : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;
  [SerializeField] private Button _buttonYes;
  [SerializeField] private Button _buttonNo;

  private ColorBlock _originalColors;
  private ColorBlock _disColors;
  private float _timeOpen = 0.5f;

  private void Awake()
  {
    if (_gameBuilder == null)
      _gameBuilder = FindObjectOfType<AbstractGameBuilder>();

    if (_gameBuilder == null)
      Debug.LogError("AbstractGameBuilder не найден на сцене!");

    _timeOpen = _gameBuilder.ResultDisplayTime;

    _originalColors = _buttonYes.colors;
    _disColors = _originalColors;
    _disColors.disabledColor = Color.white;

    _buttonYes.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonYes, "Да")));
    _buttonNo.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonNo, "Нет")));
  }

  private IEnumerator SetAnswerCoroutine(Button button, string answer)
  {
    button.colors = _disColors;

    _gameBuilder.SetAnswer(answer);

    yield return new WaitForSeconds(_timeOpen - 0.05f);

    button.colors = _originalColors;
  }
}
