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

  private Dictionary<string, Button> _cellToButtonMap = new(); // ссылки на все кнопкм
  private List<Button> _targetButtonList = new(); // список записанных кнопок
  private float _timeOpenIncorrectButton = 0.5f; // время отображения ошибки на ячейке

  //SpecificData
  private int _rows = 2;
  private int _columns = 2;

  private void Awake()
  {
    InitGameMetadata("MatrixMemory", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(11); // 7*7

    InitOptionalGameParams(
      streakCorrectQuestions: 1, // 2 для тестов
      mistakesToLevelDown: 3, // 3,
      countAnswers: 2, // 2
      countMistakes: 0, // 2
      timeMemorize: 2f, // 1f
      timeForget: 2f, // 0f
      timeAnswer: 0f, // 10f
      resultDisplayTime: 1f); // 1f
  }

  protected override void CalculateLevelConfig(int level)
  {
    level = Mathf.Clamp(level, 1, MaxLevel);

    // округляем в большую сторону и прибавляем 1 для вычисления размерности
    int dimension = (int)Math.Ceiling((double)level / 2) + 1;
    _rows = dimension;

    // если это нечётный уровень, то это квадратная матрица
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
    TextAnswer = null; // очищаем, т.к. всегда конкатенируем

    CreateMatrix();

    //QuestionData questionData = new QuestionData(
    //  TextQuestion, 
    //  _textAnswer, 
    //  _timeHideButtons * 2, 
    //  GetSpecificData(false));
    //// _timeHideButtons * 2 т.к. корутина 2 раза выполняется

    //return questionData;
  }

  // создаём сетку
  private void CreateMatrix()
  {
    _cellToButtonMap.Clear();
    _targetButtonList.Clear();

    foreach (Transform child in _matrixGridLayout.transform)
    {
      Destroy(child.gameObject);
    }
    _matrixGridLayout.constraintCount = _columns; // т.к. фикс на колонках стоит

    List<Button> buttonsList = new List<Button>(_rows * _columns);
    for (int i = 0; i < _rows * _columns; i++)
    {
      Button button = Instantiate(_prefabCell, _matrixGridLayout.transform).GetComponent<Button>();
      button.name = i.ToString(); // вроде можно убрать
      string cell = (i / _columns) + "," + (i % _columns);
      _cellToButtonMap[cell] = button; // запоминаем значение и саму кнопку
      button.GetComponent<ButtonAnswerDinamic>().InitializeButton(this, cell);
      buttonsList.Add(button);
    }

    // иначе не успевает заполнить вопрос до конца, т.к. ждёт 1с, а код продолжается
    //StartCoroutine(WaitSetTargetButton(buttonsList));
    SetTargetButton(buttonsList); // альтернатива
  }

  // устанавливаем целевые кнопки
  private void SetTargetButton(List<Button> buttonsList)
  {
    // список записанных кнопок (запоминаем просто номер кнопки, чтоб не лезть в компоненты)
    while (_targetButtonList.Count < CountAnswersToQuestion)
    {
      // из всех кнопок выбираем случайную
      //int numberButton = UnityEngine.Random.Range(0, buttonsList.Count);
      int numberButton = Rand.Next(0, buttonsList.Count);

      // НЕ записывали ли такую кнопку?
      if (!_targetButtonList.Contains(buttonsList[numberButton]))
      {
        _targetButtonList.Add(buttonsList[numberButton]);
        //buttonsList[numberButton].image.color = Color.green;
        buttonsList[numberButton].GetComponent<Image>().material = _materialBlue;

        string cell = (numberButton / _columns) + "," + (numberButton % _columns);
        TextAnswer += cell + "|";
      }
    }

    //StartCoroutine(HideTargetButtons());
  }

  public override IEnumerator StartMemorizePhaseCoroutine()
  {
    _gameZoneCanvasGroup.interactable = false;
    yield return new WaitForSeconds(TimeMemorizePhase); // время на запомнить

    foreach (Button button in _targetButtonList)
    {
      //button.image.color = Color.white;
      button.GetComponent<Image>().material = null;
    }
  }

  public override IEnumerator StartForgetPhaseCoroutine()
  {
    _forgetObject.SetActive(true);
    yield return new WaitForSeconds(TimeForgetPhase); // чтобы дать время забыть
    _forgetObject.SetActive(false);
    _gameZoneCanvasGroup.interactable = true;
  }

  //private IEnumerator HideTargetButtons()
  //{
  //  _gameZoneCanvasGroup.interactable = false;
  //  yield return new WaitForSeconds(TimeMemorizePhase); // время на запомнить


  //  foreach (Button button in _targetButtonList)
  //  {
  //    button.image.color = Color.white;
  //  }

  //  _forgetObject.gameObject.SetActive(true);
  //  yield return new WaitForSeconds(TimeForgetPhase); // чтобы дать время забыть


  //  _forgetObject.gameObject.SetActive(false);
  //  _gameZoneCanvasGroup.interactable = true;
  //}

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

      //EventAnswer eventAnswer = new EventAnswer(answer, isTrue);

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
