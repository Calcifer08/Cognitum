using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnswerDinamic : MonoBehaviour
{
  private Button _button;
  private AbstractGameBuilder _gameBuilder;
  private string _textAnswer;
  private float _timeOpen = 0.5f;

  private void Awake()
  {
    _button = GetComponent<Button>();
    _button.onClick.AddListener(OnClick);

    ColorBlock disColor = _button.colors;
    disColor.disabledColor = Color.white;
    _button.colors = disColor;
  }

  public void InitializeButton(AbstractGameBuilder abstractBuilder, string textAnswer)
  {
    _gameBuilder = abstractBuilder;
    _textAnswer = textAnswer;

    _timeOpen = _gameBuilder.ResultDisplayTime;
  }

  private void OnClick()
  {
    StartCoroutine(SetAnswerCoroutin());
  }


  private IEnumerator SetAnswerCoroutin()
  {
    _gameBuilder.SetAnswer(_textAnswer);

    yield return new WaitForSeconds(_timeOpen - 0.05f);
  }
}
