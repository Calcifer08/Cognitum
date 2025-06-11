using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using TMPro;
using UnityEngine;

public class MissingValueBuilder : AbstractGameBuilder
{
  [SerializeField] private TextMeshProUGUI _textExpression;

  [SerializeField] private GameObject _numberInput;
  [SerializeField] private GameObject _operationInput;

  //SpecificData
  private int _argumentCount;
  private int _mulDivOperationCount;
  private int _maxParentheses;
  private int _maxPositiveValue;

  private void Awake()
  {
    InitGameMetadata("MissingValue", "1.0.0", 60f);

    InitGameObject();

    InitGameParams(9);

    InitOptionalGameParams(
      streakCorrectQuestions: 5,
      mistakesToLevelDown: 3,
      countAnswers: 1,
      countMistakes: 0,
      timeMemorize: 0f,
      timeForget: 0f,
      timeAnswer: 0f,
      resultDisplayTime: 1f);
  }

  protected override void CalculateLevelConfig(int level)
  {
    level = Mathf.Clamp(level, 1, MaxLevel);

    Action action = level switch
    {
      1 => () => SetConfigLevel(2, 0, 0, 25),
      2 => () => SetConfigLevel(2, 1, 0, 15),
      3 => () => SetConfigLevel(3, 0, 0, 25),
      4 => () => SetConfigLevel(3, 1, 0, 25),
      5 => () => SetConfigLevel(4, 1, 0, 25),
      6 => () => SetConfigLevel(4, 1, 1, 25),
      7 => () => SetConfigLevel(5, 2, 1, 25),
      8 => () => SetConfigLevel(6, 2, 2, 25),
      9 => () => SetConfigLevel(7, 3, 2, 25),
      _ => throw new ArgumentException($"Недопустимый уровень: {level}")
    };

    action.Invoke();
  }

  private void SetConfigLevel(int argumentCount, int mulDivOperationCount, int maxParentheses, int maxPositiveValue)
  {
    _argumentCount = argumentCount;
    _mulDivOperationCount = mulDivOperationCount;
    _maxParentheses = maxParentheses;
    _maxPositiveValue = maxPositiveValue;
  }

  public override Dictionary<string, object> GetSpecificData(bool isLevelData)
  {
    if (isLevelData)
    {
      return new Dictionary<string, object>
      {
        { "ArgumentCount", _argumentCount},
        { "MulDivOperationCount", _mulDivOperationCount},
        { "MaxParentheses", _maxParentheses},
        { "MaxPositiveValue", _maxPositiveValue},
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

    CreateExpression();
  }

  public override IEnumerator StartMemorizePhaseCoroutine()
  {
    return null;
  }

  public override IEnumerator StartForgetPhaseCoroutine()
  {
    return null;
  }


  public override void SetAnswer(string answer)
  {
    bool isTrue;

    string correctAnswer = TextAnswer;

    if (int.TryParse(correctAnswer, out _))
    {
      isTrue = answer == correctAnswer;
    }
    else
    {
      isTrue = correctAnswer.Contains(answer);
    }

    AnswerData answerData = new AnswerData(answer, isTrue);

    RaiseOnSetAnswer(answerData);
  }

  public override void TimeAnswerDone() { }

  private void CreateExpression()
  {
    bool isExpressionRegenerationNeeded = true;

    while (isExpressionRegenerationNeeded)
    {
      string[] signs = GenerateSigns(_argumentCount, _mulDivOperationCount);

      bool[] parentheses = GenerateParentheses(signs, _maxParentheses);

      int[] operationOrder = GetOperationOrder(signs, parentheses);

      int?[] numbers = new int?[_argumentCount];
      int?[] intermediateValues = new int?[_argumentCount - 1];

      isExpressionRegenerationNeeded = GenerateNumbersWithPriority(numbers, intermediateValues, signs, operationOrder);

      if (isExpressionRegenerationNeeded)
      {
        continue;
      }

      string expression = BuildExpression(numbers, signs, parentheses);

      double result = Convert.ToDouble(new DataTable().Compute(expression, ""));

      if (result != intermediateValues[0])
        throw new ArgumentException("Ошибка в вычислении");

      (string hiddenExpression, string hiddenElement, bool isNumber) = HideRandomElement(expression, numbers, signs);

      TextQuestion = $"{hiddenExpression} = {result}";
      TextAnswer = hiddenElement;

      _textExpression.text = TextQuestion;
      _numberInput.SetActive(isNumber);
      _operationInput.SetActive(!isNumber);
    }
  }


  private string[] GenerateSigns(int numberOfArguments, int maxMulDivOperation)
  {
    string[] signs = new string[numberOfArguments - 1];
    int mulDivOperationCount = 0;

    for (int i = 0; i < signs.Length; i++)
    {
      if (mulDivOperationCount < maxMulDivOperation)
      {
        signs[i] = Rand.Next(2) == 0 ? "*" : "/";
        mulDivOperationCount++;
      }
      else
        signs[i] = Rand.Next(2) == 0 ? "+" : "-";
    }

    for (int i = signs.Length - 1; i > 0; i--)
    {
      int j = Rand.Next(i + 1);
      (signs[i], signs[j]) = (signs[j], signs[i]);
    }

    return signs;
  }

  private bool[] GenerateParentheses(string[] signs, int maxParentheses)
  {
    bool[] parentheses = new bool[signs.Length];

    List<int> availableIndices = new List<int>();

    for (int i = 0; i < signs.Length; i++)
      if (signs[i] == "+" || signs[i] == "-")
        availableIndices.Add(i);

    maxParentheses = Math.Min(maxParentheses, availableIndices.Count);

    List<int> chosenIndices = new List<int>();
    int attempts = 0;
    int maxAttempts = availableIndices.Count * 2;

    while (chosenIndices.Count < maxParentheses && attempts < maxAttempts)
    {
      int randomIndex = availableIndices[Rand.Next(availableIndices.Count)];

      if (!chosenIndices.Contains(randomIndex) &&
          !chosenIndices.Contains(randomIndex - 1) &&
          !chosenIndices.Contains(randomIndex + 1))
      {
        chosenIndices.Add(randomIndex);
      }

      attempts++;
    }

    foreach (int index in chosenIndices)
      parentheses[index] = true;

    return parentheses;
  }

  private int[] GetOperationOrder(string[] signs, bool[] parentheses)
  {
    List<(int index, int priority)> operations = new List<(int, int)>();

    for (int i = 0; i < signs.Length; i++)
    {
      int priority = 0;

      if (parentheses[i])
        priority += 2;

      if (signs[i] == "*" || signs[i] == "/")
        priority += 1;

      operations.Add((i, priority));
    }

    var sortedOperations = operations.OrderByDescending(op => op.priority).ThenBy(op => op.index);

    int[] order = sortedOperations.Select(op => op.index).ToArray();

    return order;
  }

  private bool GenerateNumbersWithPriority(int?[] numbers, int?[] intermediateValues, string[] signs, int[] operationOrder)
  {
    foreach (int operationIndex in operationOrder)
    {
      int? left = operationIndex == 0 ? null : intermediateValues[operationIndex - 1];
      int? right = operationIndex == operationOrder.Length - 1 ? null : intermediateValues[operationIndex + 1];

      if (left == null && right == null)
      {
        left = Rand.Next(1, _maxPositiveValue);
        right = GenerateOperand(left.Value, signs[operationIndex], isLeftOperand: true);
      }
      else if (left != null && right == null)
        right = GenerateOperand(left.Value, signs[operationIndex], isLeftOperand: true);
      else if (left == null && right != null)
        left = GenerateOperand(right.Value, signs[operationIndex], isLeftOperand: false);

      int result = PerformOperation(left.Value, right.Value, signs[operationIndex]);

      if (result <= 0 || result > _maxPositiveValue || result == int.MinValue)
        return true;

      if (numbers[operationIndex] == null)
        numbers[operationIndex] = left;

      if (numbers[operationIndex + 1] == null)
        numbers[operationIndex + 1] = right;

      UpdateIntermediateArray(intermediateValues, operationIndex, result);
    }

    return false;
  }

  private int GenerateOperand(int number, string sign, bool isLeftOperand)
  {
    switch (sign)
    {
      case "+":
        return GenerateAddition(number);
      case "-":
        return isLeftOperand ? GenerateSubtraction(number) : GenerateAddition(number) + number;
      case "*":
        return isLeftOperand ? GenerateMultiplier(number) : GenerateMultiplier(number);
      case "/":
        return isLeftOperand ? GenerateDivision(number) : GenerateMultiplier(number);
      default:
        throw new ArgumentException("Неподдерживаемая операция");
    }
  }

  private int PerformOperation(int left, int right, string sign)
  {
    switch (sign)
    {
      case "+":
        return left + right;
      case "-":
        return left - right;
      case "*":
        return left * right;
      case "/":
        if (right == 0 || left % right != 0)
          return int.MinValue;
        return left / right;
      default:
        throw new ArgumentException("Неподдерживаемая операция");
    }
  }

  private void UpdateIntermediateArray(int?[] intermediateValues, int index, int newValue)
  {
    intermediateValues[index] = newValue;

    for (int i = index - 1; i >= 0 && intermediateValues[i] != null; i--)
      intermediateValues[i] = newValue;

    for (int i = index + 1; i < intermediateValues.Length && intermediateValues[i] != null; i++)
      intermediateValues[i] = newValue;
  }

  private string BuildExpression(int?[] numbers, string[] signs, bool[] parentheses)
  {
    if (numbers.Length != signs.Length + 1 || signs.Length != parentheses.Length)
      throw new ArgumentException("Несоответствие длин массивов numbers, signs и parentheses.");

    StringBuilder expression = new StringBuilder();

    if (parentheses[0])
      expression.Append($"({numbers[0]}");
    else
      expression.Append($"{numbers[0]}");

    for (int i = 0; i < signs.Length; i++)
    {
      if (parentheses[i])
        expression.Append($" {signs[i]} {numbers[i + 1]})");
      else if (i + 1 < parentheses.Length && parentheses[i + 1])
        expression.Append($" {signs[i]} ({numbers[i + 1]}");
      else 
        expression.Append($" {signs[i]} {numbers[i + 1]}");
    }

    return expression.ToString();
  }

  private (string hiddenExpression, string hiddenElement, bool isNumber) HideRandomElement(string expression, int?[] numbers, string[] signs)
  {
    string[] numberStrings = numbers
        .Where(n => n.HasValue)
        .Select(n => n.Value.ToString())
        .ToArray();

    string[][] targetSets = Rand.Next(2) == 0
        ? new string[][] { numberStrings, signs }
        : new string[][] { signs, numberStrings };

    for (int i = 0; i < targetSets.Length; i++)
    {
      List<(int Index, string Value)> matches = new List<(int, string)>();

      for (int j = 0; j < targetSets[i].Length; j++)
      {
        string pattern = ReferenceEquals(targetSets[i], numberStrings)
          ? $@"\b{Regex.Escape(targetSets[i][j])}\b"
          : $@" {Regex.Escape(targetSets[i][j])} ";

        MatchCollection foundMatches = Regex.Matches(expression, pattern);

        foreach (Match match in foundMatches.Cast<Match>())
        {
          matches.Add((match.Index, match.Value));
        }
      }

      if (matches.Count > 0)
      {
        bool isNumber = ReferenceEquals(targetSets[i], numberStrings);
        var selected = matches[Rand.Next(matches.Count)];

        string hiddenElement = selected.Value.Trim();
        string hiddenExpression;

        if (isNumber)
        {
          hiddenExpression = expression.Substring(0, selected.Index) +
                           "?" +
                           expression.Substring(selected.Index + selected.Value.Length);
          return (hiddenExpression, hiddenElement, isNumber);
        }
        else
        {
          string originalResult = new DataTable().Compute(expression, "").ToString();

          string[] possibleSigns = { "+", "-", "*", "/" };

          List<string> matchingSigns = new List<string>();

          StringBuilder sb = new StringBuilder(expression);

          foreach (var sign in possibleSigns)
          {
            sb[selected.Index + 1] = sign[0];

            string modifiedExpression = sb.ToString();
            string resultWithSign = new DataTable().Compute(modifiedExpression, "").ToString();

            if (resultWithSign == originalResult)
            {
              matchingSigns.Add(sign);
            }
          }

          if (matchingSigns.Count > 0)
          {
            hiddenExpression = expression.Substring(0, selected.Index) +
                                  " ? " +
                                  expression.Substring(selected.Index + selected.Value.Length);

            return (hiddenExpression, string.Join(", ", matchingSigns), isNumber);
          }
        }
      }
    }

    throw new ArgumentException("Скрыть элемент не удалось. Нет совпадений.");
  }

  private static string GetRightOperand(string expression, int startIndex)
  {
    expression = expression.Substring(startIndex + 1).Trim();

    Match match = Regex.Match(expression, @"^\(?\d+(\s*[-+*/]\s*\d+)*\)?");

    if (match.Success)
    {
      return match.Value;
    }

    throw new Exception("Не удалось определить правый операнд.");
  }

  private int GenerateAddition(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    if (leftOperand >= _maxPositiveValue)
      return 1;

    int maxPossible = _maxPositiveValue - leftOperand;

    return Rand.Next(1, maxPossible + 1);
  }

  private int GenerateSubtraction(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    if (leftOperand <= 1)
      return 0;

    int minValue = Math.Max(1, leftOperand - _maxPositiveValue);

    return Rand.Next(minValue, leftOperand);
  }

  private int GenerateMultiplier(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    int maxMultiplication = _maxPositiveValue / leftOperand;

    if (maxMultiplication <= 2)
      return 1;

    return Rand.Next(2, maxMultiplication + 1);
  }

  private int GenerateDivision(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    int maxDivision = _maxPositiveValue / 2;

    if (leftOperand == 0)
      return Rand.Next(2, maxDivision + 1);

    if (leftOperand == 1)
      return 1;

    List<int> divisors = new List<int>();

    for (int i = 2; i <= maxDivision; i++)
      if (leftOperand % i == 0)
        divisors.Add(i);

    return divisors.Count > 0 ? divisors[Rand.Next(divisors.Count)] : leftOperand;
  }
}
