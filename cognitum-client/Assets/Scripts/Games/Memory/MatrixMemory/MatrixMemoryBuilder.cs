using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatrixMemoryBuilder : AbstractGameBuilder
{
  [SerializeField] private GridLayoutGroup _matrixGridLayout;
  [SerializeField] private GameObject _prefabCell;

  [SerializeField] private Material _materialBlue;
  [SerializeField] private Material _materialRed;

  private Dictionary<string, Button> _cellToButtonMap = new();
  private List<Button> _targetButtonList = new();
  private float _timeOpenIncorrectButton = 0.5f;

  //SpecificData
  private int _rows = 2;
  private int _columns = 2;

  private void Awake()
  {
    InitGameMetadata("MatrixMemory", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(11);

    InitOptionalGameParams(
      streakCorrectQuestions: 1,
      mistakesToLevelDown: 3,
      countAnswers: 2,
      countMistakes: 0,
      timeMemorize: 2f,
      timeForget: 2f,
      timeAnswer: 0f,
      resultDisplayTime: 1f);
  }

  protected override void CalculateLevelConfig(int level)
  {
    level = Mathf.Clamp(level, 1, MaxLevel);

    int dimension = (int)Math.Ceiling((double)level / 2) + 1;
    _rows = dimension;

    if (level % 2 == 1)
    {
      _columns = dimension;
    }
    else
    {
      _columns = dimension + 1;
    }

    CountAnswersToQuestion = (_rows * _columns) / 2;

    TextQuestion = $"Размерность матрицы {_rows} * {_columns}. Открыть зелёные ячейки. Число ячеек: {CountAnswersToQuestion}";
  }

  public override Dictionary<string, object> GetSpecificData(bool isLevelData)
  {
    if (isLevelData)
    {
      return new Dictionary<string, object>
      {
        { "Rows", _rows},
        { "Columns", _columns}
      };
    }
    else
    {
      return new Dictionary<string, object>();
    }
  }

  public override void GameUpdateQuestion()
  {
    TextAnswer = null;

    CreateMatrix();
  }

  private void CreateMatrix()
  {
    _cellToButtonMap.Clear();
    _targetButtonList.Clear();

    foreach (Transform child in _matrixGridLayout.transform)
    {
      Destroy(child.gameObject);
    }
    _matrixGridLayout.constraintCount = _columns;

    List<Button> buttonsList = new List<Button>(_rows * _columns);
    for (int i = 0; i < _rows * _columns; i++)
    {
      Button button = Instantiate(_prefabCell, _matrixGridLayout.transform).GetComponent<Button>();
      button.name = i.ToString();
      string cell = (i / _columns) + "," + (i % _columns);
      _cellToButtonMap[cell] = button;
      button.GetComponent<ButtonAnswerDinamic>().InitializeButton(this, cell);
      buttonsList.Add(button);
    }

    SetTargetButton(buttonsList);
  }

  private void SetTargetButton(List<Button> buttonsList)
  {
    while (_targetButtonList.Count < CountAnswersToQuestion)
    {
      int numberButton = Rand.Next(0, buttonsList.Count);

      if (!_targetButtonList.Contains(buttonsList[numberButton]))
      {
        _targetButtonList.Add(buttonsList[numberButton]);
        buttonsList[numberButton].GetComponent<Image>().material = _materialBlue;

        string cell = (numberButton / _columns) + "," + (numberButton % _columns);
        TextAnswer += cell + "|";
      }
    }
  }

  public override IEnumerator StartMemorizePhaseCoroutine()
  {
    _gameZoneCanvasGroup.interactable = false;
    yield return new WaitForSeconds(TimeMemorizePhase);

    foreach (Button button in _targetButtonList)
    {
      button.GetComponent<Image>().material = null;
    }
  }

  public override IEnumerator StartForgetPhaseCoroutine()
  {
    _forgetObject.SetActive(true);
    yield return new WaitForSeconds(TimeForgetPhase);
    _forgetObject.SetActive(false);
    _gameZoneCanvasGroup.interactable = true;
  }

  public override void SetAnswer(string answer)
  {
    if (_cellToButtonMap.TryGetValue(answer, out Button button))
    {
      button.interactable = false;
      bool isTrue;

      if (_targetButtonList.Contains(button))
      {
        button.GetComponent<Image>().material = _materialBlue;
        isTrue = true;
      }
      else
      {
        button.GetComponent<Image>().material = _materialRed;
        StartCoroutine(BlockButtonAfterClicking(button, _timeOpenIncorrectButton));
        isTrue = false;
      }

      AnswerData answerData = new AnswerData(answer, isTrue);

      RaiseOnSetAnswer(answerData);
    }
    else
    {
      Debug.LogError($"Нет кнопки с координатами {answer}");
    }
  }
  private IEnumerator BlockButtonAfterClicking(Button button, float time)
  {
    yield return new WaitForSeconds(time);
    button.GetComponent<Image>().material = null;
    button.interactable = true;
  }

  public override void TimeAnswerDone() { }
}
