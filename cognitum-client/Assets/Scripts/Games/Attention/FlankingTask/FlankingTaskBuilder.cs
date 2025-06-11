using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FlankingTaskBuilder : AbstractGameBuilder
{
  private static class FlankerLayout
  {
    public const string Horizontal = "Horizontal";
    public const string Vertical = "Vertical";
    public const string Cross = "Cross";
  }

  [SerializeField] private GameObject _flankerPrefab;
  [SerializeField] private GameObject _targetFlanker;
  [SerializeField] private List<Transform> _horizontalPositions;
  [SerializeField] private List<Transform> _verticalPositions;
  [SerializeField] private Image _imageInvertDirection;

  private List<string> _layoutTypeList;

  private string _layoutType;

  private float _invertDirectionChance = 0f;

  private bool _isInvert = false;

  private void Awake()
  {
    InitGameMetadata("FlankingTask", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(6);

    InitOptionalGameParams(
      streakCorrectQuestions: 7,
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

    Action action = level switch
    {
      1 => () => SetConfigLevel(0f, 3f, new List<string> { FlankerLayout.Horizontal }),
      2 => () => SetConfigLevel(0f, 3f, new List<string> { FlankerLayout.Vertical }),
      3 => () => SetConfigLevel(0f, 2.75f, new List<string> { FlankerLayout.Cross }),
      4 => () => SetConfigLevel(0f, 2.5f, new List<string> { FlankerLayout.Horizontal,
        FlankerLayout.Vertical }),
      5 => () => SetConfigLevel(0f, 2f, new List<string> { FlankerLayout.Horizontal,
        FlankerLayout.Vertical, FlankerLayout.Cross }),
      6 => () => SetConfigLevel(0.5f, 2f, new List<string> { FlankerLayout.Horizontal,
        FlankerLayout.Vertical, FlankerLayout.Cross }),
      _ => throw new ArgumentException($"Недопустимый уровень: {level}")
    };

    action.Invoke();
  }

  private void SetConfigLevel(float invertDirectionChance, float timeAnswerPhase, List<string> layouts)
  {
    _invertDirectionChance = invertDirectionChance;
    TimeAnswerPhase = timeAnswerPhase;
    _layoutTypeList = layouts;
  }

  public override Dictionary<string, object> GetSpecificData(bool isLevelData)
  {
    if (isLevelData)
    {
      string layoutTypes = string.Join(", ", _layoutTypeList);
      return new Dictionary<string, object>
      {
        { "LayoutTypes", layoutTypes },
        { "InvertDirectionChance", _invertDirectionChance }

      };
    }
    else
    {
      return new Dictionary<string, object>
      {
        { "LayoutType", _layoutType },
        { "IsInvert", _isInvert }
      };
    }
  }

  public override void SetAnswer(string answer)
  {
    bool isTrue = answer.ToLower() == TextAnswer.ToLower();

    AnswerData answerData = new AnswerData(answer, isTrue);

    RaiseOnSetAnswer(answerData);
  }

  public override void TimeAnswerDone() { }

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
    ClearArrowsFromPositions(_horizontalPositions);
    ClearArrowsFromPositions(_verticalPositions);

    _layoutType = _layoutTypeList[Rand.Next(0, _layoutTypeList.Count)];

    string targetDirection = GetRandomDirection();

    RotateArrow(_targetFlanker, targetDirection);

    _isInvert = Rand.NextDouble() < _invertDirectionChance;
    string finalTargetDirection;

    if (_isInvert)
    {
      finalTargetDirection = GetOppositeDirection(targetDirection);
      _imageInvertDirection.canvasRenderer.SetAlpha(1f);
    }
    else
    {
      finalTargetDirection = targetDirection;
      _imageInvertDirection.canvasRenderer.SetAlpha(0f);
    }

    List<Transform> positions = GetPositionsByLayout(_layoutType);

    bool isFlankerDirection = Rand.NextDouble() < 0.5;
    string flankerDirection = isFlankerDirection ? finalTargetDirection : GetOppositeDirection(finalTargetDirection);

    foreach (var pos in positions)
    {
      GameObject arrow = Instantiate(_flankerPrefab, pos.position, Quaternion.identity, pos);
      RotateArrow(arrow, flankerDirection);
    }

    TextAnswer = finalTargetDirection;
    TextQuestion = $"Целевое направление: {targetDirection}. " +
      $"Инверсия ответа: {_isInvert}. " +
      $"Направление фланкеров: {flankerDirection}.";
  }


  private void ClearArrowsFromPositions(List<Transform> positions)
  {
    foreach (Transform container in positions)
    {
      if (container.childCount > 0)
      {
        Destroy(container.GetChild(0).gameObject);
      }
    }
  }

  private string GetRandomDirection()
  {
    string[] directions = { Directions.Up, Directions.Down, Directions.Left, Directions.Right };
    return directions[Rand.Next(0, directions.Length)];
  }

  private string GetOppositeDirection(string direction)
  {
    return direction switch
    {
      Directions.Up => Directions.Down,
      Directions.Down => Directions.Up,
      Directions.Left => Directions.Right,
      Directions.Right => Directions.Left,
      _ => direction
    };
  }

  private List<Transform> GetPositionsByLayout(string layout)
  {
    return layout switch
    {
      FlankerLayout.Horizontal => _horizontalPositions,
      FlankerLayout.Vertical => _verticalPositions,
      FlankerLayout.Cross => _horizontalPositions.Concat(_verticalPositions).ToList(),
      _ => throw new ArgumentException($"Неизвестный макет: {layout}")
    };
  }

  private void RotateArrow(GameObject arrow, string direction)
  {
    float zRotation = direction switch
    {
      Directions.Up => 90f,
      Directions.Right => 0f,
      Directions.Down => 270f,
      Directions.Left => 180f,
      _ => 0f
    };
    arrow.transform.rotation = Quaternion.Euler(0, 0, zRotation);
  }
}
