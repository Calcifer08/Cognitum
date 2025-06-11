using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NBackBuilder : AbstractGameBuilder
{
  //SpecificData
  private int _countPositions;
  private int _countHiddenPositions;
  private int _countMemorySpan;

 
  [SerializeField] private Transform _sequenceParent;
  [SerializeField] private GameObject _numberPrefab;

  private int _targetHiddenIndex = 0;
  private int _targetCompareIndex;

  private int _currentHidden = -1;
  private List<int> _sequence = new();

  private void Awake()
  {
    InitGameMetadata("NBack", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(10);

    InitOptionalGameParams(
      streakCorrectQuestions: 7,
      mistakesToLevelDown: 3,
      countAnswers: 1,
      countMistakes: 0,
      timeMemorize: 0f,
      timeForget: 0f,
      timeAnswer: 0f,
      resultDisplayTime: 0.5f);
  }

  protected override void CalculateLevelConfig(int level)
  {
    level = Mathf.Clamp(level, 1, MaxLevel);

    Action action = level switch
    {
      1 => () => SetLevelConfig(3, 1, 2, 0f),
      2 => () => SetLevelConfig(3, 1, 2, 3f),
      3 => () => SetLevelConfig(3, 1, 2, 2.5f),
      4 => () => SetLevelConfig(3, 1, 2, 2f),
      5 => () => SetLevelConfig(3, 1, 2, 1.5f),
      6 => () => SetLevelConfig(4, 1, 3, 3f),
      7 => () => SetLevelConfig(4, 1, 3, 3f),
      8 => () => SetLevelConfig(4, 1, 3, 2f),
      9 => () => SetLevelConfig(4, 2, 3, 4f),
      10 => () => SetLevelConfig(4, 2, 3, 3f),
      _ => throw new ArgumentException($"Недопустимый уровень: {level}")
    };

    action.Invoke();
  }

  private void SetLevelConfig(int positions, int hiddenPositions, int memorySpan, float timeLimit)
  {
    _countPositions = positions;
    _countHiddenPositions = hiddenPositions;
    _countMemorySpan = memorySpan;
    TimeAnswerPhase = timeLimit;

    _sequence.Clear();
    _currentHidden = -1;

    for (int i = 0; i < _countPositions - 1; i++)
    {
      _sequence.Add(Rand.Next(1, 10));
    }
  }

  public override Dictionary<string, object> GetSpecificData(bool isLevelData)
  {
    if (isLevelData)
    {
      return new Dictionary<string, object>
      {
        { "Positions", _countPositions },
        { "HiddenPositions", _countHiddenPositions },
        { "MemorySpan", _countMemorySpan },
      };
    }
    else
    {
      return new Dictionary<string, object>
      {
        { "CurrentPositions", _currentHidden },
      };
    }
  }

  public override void SetAnswer(string answer)
  {
    bool isTrue = answer.ToLower() == TextAnswer.ToLower();

    if (!isTrue)
      _currentHidden = -1;

    AnswerData answerData = new AnswerData(answer, isTrue);

    RaiseOnSetAnswer(answerData);
  }

  public override void TimeAnswerDone()
  {
    _currentHidden = -1;
  }

  public override IEnumerator StartForgetPhaseCoroutine()
  {
    return null;
  }

  public override IEnumerator StartMemorizePhaseCoroutine()
  {
    return null;
  }

  public override void GameUpdateQuestion()
  {
    if (_sequence.Count >= _countMemorySpan && Rand.NextDouble() < 0.5)
    {
      _sequence.Add(_sequence[_sequence.Count - _countMemorySpan]);
    }
    else
    {
      _sequence.Add(Rand.Next(1, 10));
    }

    if (_sequence.Count > _countPositions)
      _sequence.RemoveAt(0);

    if (_currentHidden < _countHiddenPositions)
      _currentHidden++;

    _targetCompareIndex = _targetHiddenIndex + _countMemorySpan;
    TextAnswer = _sequence[_targetHiddenIndex] == _sequence[_targetCompareIndex] ? "да" : "нет";
    TextQuestion =
      $"Последовательность: [{string.Join(", ", _sequence)}] | " +
      $"Скрытые позиции до: {_currentHidden} (включительно) | " +
      $"Сравниваем индексы: {_targetHiddenIndex} и {_targetCompareIndex} | " +
      $"Совпадают ли числа на этих позициях?";

    ShowVisibleNumbers(_currentHidden);
  }

  private void ShowVisibleNumbers(int hiddenCount)
  {
    foreach (Transform child in _sequenceParent)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < _sequence.Count; i++)
    {
      int number = _sequence[i];

      GameObject obj = Instantiate(_numberPrefab, _sequenceParent);

      TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = number.ToString();

      if (i < hiddenCount)
      {
        tmp.canvasRenderer.SetAlpha(0f);
      }

      if (i == _targetHiddenIndex || i == _targetCompareIndex)
      {
        Image img = obj.GetComponentInChildren<Image>();

        if (img != null)
        {
          img.color = new Color32(112, 159, 243, 255);
        }
      }
    }
  }
}
