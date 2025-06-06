using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnswerInputSymbol : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;

  [SerializeField] private Button _buttonPlus;
  [SerializeField] private Button _buttonMinus;
  [SerializeField] private Button _buttonMultiplication;
  [SerializeField] private Button _buttonDivision;

  private ColorBlock _originalColors;
  private ColorBlock _disColors;
  private float _timeOpen = 0.5f;

  private void Awake()
  {
    _timeOpen = _gameBuilder.ResultDisplayTime;

    _originalColors = _buttonPlus.colors;
    _disColors = _originalColors;
    _disColors.disabledColor = Color.white;

    _buttonPlus.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonPlus, "+")));
    _buttonMinus.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonMinus, "-")));
    _buttonMultiplication.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonMultiplication, "*")));
    _buttonDivision.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonDivision, "/")));
  }

  private IEnumerator SetAnswerCoroutine(Button button, string symbol)
  {
    button.colors = _disColors;

    _gameBuilder.SetAnswer(symbol);

    yield return new WaitForSeconds(_timeOpen - 0.05f);

    button.colors = _originalColors;
  }
}
