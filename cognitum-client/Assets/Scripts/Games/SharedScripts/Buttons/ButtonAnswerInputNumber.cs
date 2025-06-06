using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnswerInputNumber : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;

  [SerializeField] private TextMeshProUGUI _answerField;
  [SerializeField] private Button _buttonSetAnswer;
  [SerializeField] private Button _buttonClearAnswer;
  [SerializeField] private List<Button> _buttonList;
  private float _timeOpen = 0.5f;
  private string _textAnswer = "";
  public int MaxSymbolCount = 3;

  private void Awake()
  {
    ClearAnswer();

    for (int i = 0; i < _buttonList.Count; i++)
    {
      int index = i; // если не сделать локалку, то все лямбды будут ссылаться на одно и то же значение

      _buttonList[index].onClick.AddListener(() => UpdateAnswerText(index.ToString()[0]));
    }

    _buttonSetAnswer.onClick.AddListener(() => StartCoroutine(SetAnswerCoroutine()));
    _buttonClearAnswer.onClick.AddListener(ClearAnswer);

    _timeOpen = _gameBuilder.ResultDisplayTime;

    ColorBlock disColor = _buttonSetAnswer.colors;
    disColor.disabledColor = Color.white;
    _buttonSetAnswer.colors = disColor;
  }

  public void UpdateAnswerText(char textAnswer)
  {
    if (_answerField.text.Length < MaxSymbolCount)
    {
      _textAnswer += textAnswer;
      _answerField.text = _textAnswer;
    }
  }

  private IEnumerator SetAnswerCoroutine()
  {
    _gameBuilder.SetAnswer(_textAnswer);

    yield return new WaitForSeconds(_timeOpen - 0.05f);

    ClearAnswer();
  }

  private void ClearAnswer()
  {
    _textAnswer = "";
    _answerField.text = _textAnswer;
  }
}
