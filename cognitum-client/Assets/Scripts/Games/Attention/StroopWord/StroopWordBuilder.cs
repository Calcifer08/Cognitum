using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StroopWordBuilder : AbstractGameBuilder
{
  [SerializeField] private TextMeshProUGUI _questionText;

  private readonly List<string> _colorNames = new List<string>
  {
    "Красный", "Зелёный", "Синий", "Жёлтый",
    "Оранжевый", "Фиолетовый", "Голубой", "Чёрный"
  };

  private readonly Dictionary<string, Color32> _colorDict = new Dictionary<string, Color32>
  {
    { "Красный", new Color32(239, 53, 68, 255) },
    { "Зелёный", new Color32(140, 214, 173, 255) },
    { "Синий", new Color32(63, 134, 242, 255) },
    { "Жёлтый", new Color32(255, 236, 114, 255) },
    { "Оранжевый", new Color32(254, 153, 0, 255) },
    { "Фиолетовый", new Color32(151, 105, 204, 255) },
    { "Голубой", new Color32(173, 213, 245, 255) },
    { "Чёрный", new Color32(25, 30, 38, 255) },
  };

  private int _minColors = 3;
  private float _maxTime = 3f;
  private float _minTime = 1f;
  private double _chanceMatch = 0.5;

  private int _colorCount;

  private void Awake()
  {
    InitGameMetadata("StroopWord", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(6);

    InitOptionalGameParams(
      streakCorrectQuestions: 5,
      mistakesToLevelDown: 3,
      countAnswers: 1,
      countMistakes: 0,
      timeMemorize: 0f,
      timeForget: 0f,
      timeAnswer: 3f,
      resultDisplayTime: 0.5f);
  }

  protected override void CalculateLevelConfig(int level)
  {
    level = Mathf.Clamp(level, 1, MaxLevel);

    _colorCount = Mathf.Clamp(_minColors + (level - 1), _minColors, _colorNames.Count);

    float step = (_maxTime - _minTime) / (MaxLevel - 1);
    TimeAnswerPhase = _maxTime - step * (level - 1);

    Debug.Log($"Level {level}: Цветов: {_colorCount}, Время на ответ: {TimeAnswerPhase} секунд");
  }

  public override Dictionary<string, object> GetSpecificData(bool isLevelData)
  {
    if (isLevelData)
    {
      string colors = string.Join(", ", _colorNames);
      return new Dictionary<string, object>()
      {
        {"Colors", colors }
      };
    }
    else
    {
      return new Dictionary<string, object>();
    }
  }

  public override void GameUpdateQuestion()
  {
    bool isMatch = Rand.NextDouble() < _chanceMatch;

    string word;
    string colorName;
    int index = Rand.Next(_colorCount);

    if (isMatch)
    {
      word = _colorNames[index];
      colorName = _colorNames[index];
    }
    else
    {
      int colorIndex;
      do
      {
        colorIndex = Rand.Next(_colorCount);
      } while (colorIndex == index);

      word = _colorNames[index];
      colorName = _colorNames[colorIndex];
    }

    Color color = _colorDict[colorName];

    _questionText.text = word;
    _questionText.color = color;

    TextQuestion = $"Текст: {word}, Цвет: {colorName}";

    TextAnswer = (word == colorName) ? "Да" : "Нет";
  }

  public override IEnumerator StartMemorizePhaseCoroutine() { return null; }

  public override IEnumerator StartForgetPhaseCoroutine() { return null; }

  public override void SetAnswer(string answer)
  {
    bool isTrue = answer.ToLower() == TextAnswer.ToLower();

    AnswerData answerData = new AnswerData(answer, isTrue);

    RaiseOnSetAnswer(answerData);
  }

  public override void TimeAnswerDone() {}
}
