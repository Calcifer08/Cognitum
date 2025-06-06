using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDirection : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;

  [SerializeField] private Button _buttonUp;
  [SerializeField] private Button _buttonDown;
  [SerializeField] private Button _buttonLeft;
  [SerializeField] private Button _buttonRight;

  private ColorBlock _originalColors;
  private ColorBlock _disColors;
  private float _timeOpen = 0.5f;

  private void Awake()
  {
    _timeOpen = _gameBuilder.ResultDisplayTime;

    _originalColors = _buttonUp.colors;
    _disColors = _originalColors;
    _disColors.disabledColor = Color.white;

    _buttonUp.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonUp, Directions.Up)));
    _buttonDown.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonDown, Directions.Down)));
    _buttonLeft.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonLeft, Directions.Left)));
    _buttonRight.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine(_buttonRight, Directions.Right)));
  }

  private IEnumerator SetAnswerCoroutine(Button button, string direction)
  {
    button.colors = _disColors;

    _gameBuilder.SetAnswer(direction);

    yield return new WaitForSeconds(_timeOpen - 0.05f);

    button.colors = _originalColors;
  }
}

public static class Directions
{
  public const string Up = "Up";
  public const string Down = "Down";
  public const string Left = "Left";
  public const string Right = "Right";
}
