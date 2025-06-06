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
  private int _positions; // Количество позиций
  private int _hiddenPositions; // Количество скрытых позиций
  private int _memorySpan; // Количество позиций для запоминания

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
    _positions = positions;
    _hiddenPositions = hiddenPositions;
    _memorySpan = memorySpan;
    TimeAnswerPhase = timeLimit;

    _sequence.Clear(); // очищаем при смене уровня
    _currentHidden = -1;

    for (int i = 0; i < _positions - 1; i++)
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
        { "Positions", _positions },
        { "HiddenPositions", _hiddenPositions },
        { "MemorySpan", _memorySpan },
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







  private int _targetHiddenIndex = 0; // скрытое
  private int _targetCompareIndex;

  private int _currentHidden = -1;
  private List<int> _sequence = new();

  public override void GameUpdateQuestion()
  {
    // Добавляем новое число
    if (_sequence.Count >= _memorySpan && Rand.NextDouble() < 0.5)
    {
      _sequence.Add(_sequence[_sequence.Count - _memorySpan]);
    }
    else
    {
      // есть малая вероятность что совпадёт, но не страшно
      _sequence.Add(Rand.Next(1, 10));
    }

    // Удаляем самое старое число, чтобы не превышать длину
    if (_sequence.Count > _positions)
      _sequence.RemoveAt(0);

    // Постепенно увеличиваем количество скрытых чисел
    if (_currentHidden < _hiddenPositions)
      _currentHidden++;

    _targetCompareIndex = _targetHiddenIndex + _memorySpan;
    TextAnswer = _sequence[_targetHiddenIndex] == _sequence[_targetCompareIndex] ? "да" : "нет";
    TextQuestion =
      $"Последовательность: [{string.Join(", ", _sequence)}] | " +
      $"Скрытые позиции до: {_currentHidden} (включительно) | " +
      $"Сравниваем индексы: {_targetHiddenIndex} и {_targetCompareIndex} | " +
      $"Совпадают ли числа на этих позициях?";

    // Обновляем UI
    ShowVisibleNumbers(_currentHidden);
  }

  [SerializeField] private Transform _sequenceParent; // Родительский объект, куда спавним числа
  [SerializeField] private GameObject _numberPrefab; // Префаб цифры

  private void ShowVisibleNumbers(int hiddenCount)
  {
    // Удаляем все предыдущие числа
    foreach (Transform child in _sequenceParent)
    {
      Destroy(child.gameObject);
    }

    // Спавним числа заново по текущей последовательности
    for (int i = 0; i < _sequence.Count; i++)
    {
      int number = _sequence[i];

      // Спавним префаб
      GameObject obj = Instantiate(_numberPrefab, _sequenceParent);

      // Устанавливаем текст
      TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = number.ToString();

      // Прячем, если входит в скрываемые (слева)
      if (i < hiddenCount)
      {
        tmp.canvasRenderer.SetAlpha(0f);
      }

      // Подсвечиваем цветом Image, если это сравниваемые позиции
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
