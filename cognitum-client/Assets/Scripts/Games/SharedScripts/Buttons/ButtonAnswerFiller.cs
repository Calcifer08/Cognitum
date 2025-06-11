using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnswerFiller : MonoBehaviour
{
  [SerializeField] private AbstractGameBuilder _gameBuilder;
  [SerializeField] private Button _buttonAnswer;
  [SerializeField] private Button _buttonClearAnswer;
  private string _textAnswer;

  private void Start()
  {
    _buttonAnswer.onClick.AddListener(() => SetAnswer());
    _buttonClearAnswer.onClick.AddListener(ClearAnswer);
  }

  public void UpdateAnswerText(char textAnswer)
  {
    _textAnswer += textAnswer;
  }

  private void SetAnswer()
  {
    _gameBuilder.SetAnswer(_textAnswer);
    ClearAnswer();
  }

  private void ClearAnswer()
  {
    _textAnswer = null;
  }
}
