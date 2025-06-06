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

  private int _argumentCount;  // Количество аргументов
  private int _mulDivOperationCount;             // Максимальное количество знаков * и /
  private int _maxParentheses;     // Максимальное количество скобок
  private int _maxPositiveValue;   // максимальное положительное число при генерации

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
    // ограничиваем, если передели не тот
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
    TextAnswer = null; // очищаем, т.к. всегда конкатенируем

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

    // определяем число или знак
    if (int.TryParse(correctAnswer, out _))
    {
      isTrue = answer == correctAnswer;
    }
    else
    {
      isTrue = correctAnswer.Contains(answer);
    }

    //EventAnswer eventAnswer = new EventAnswer(answer, isTrue);

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

      // (можно убрать и брать любой элемент массива intermediateValues, но он помогает находить ошибки)
      double result = Convert.ToDouble(new DataTable().Compute(expression, ""));

      if (result != intermediateValues[0])
        throw new ArgumentException("Ошибка в вычислении");

      // скрываем элемент
      (string hiddenExpression, string hiddenElement, bool isNumber) = HideRandomElement(expression, numbers, signs);

      TextQuestion = $"{hiddenExpression} = {result}";
      TextAnswer = hiddenElement;

      _textExpression.text = TextQuestion;
      _numberInput.SetActive(isNumber);
      _operationInput.SetActive(!isNumber);
    }
  }






  // генерируем знаки
  private string[] GenerateSigns(int numberOfArguments, int maxMulDivOperation)
  {
    string[] signs = new string[numberOfArguments - 1];
    int mulDivOperationCount = 0;

    // сначала заполним * и /, потом + и -
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

    // перемешаем
    for (int i = signs.Length - 1; i > 0; i--)
    {
      int j = Rand.Next(i + 1);
      (signs[i], signs[j]) = (signs[j], signs[i]);
    }

    return signs;
  }

  // генерируем скобки
  private bool[] GenerateParentheses(string[] signs, int maxParentheses)
  {
    // сколько операций, столько и позииций для скобок
    bool[] parentheses = new bool[signs.Length];

    // сколько может скобок быть с учётом + и -
    List<int> availableIndices = new List<int>();

    for (int i = 0; i < signs.Length; i++)
      if (signs[i] == "+" || signs[i] == "-")
        availableIndices.Add(i);

    // если скобок вышло меньше, чем ожидалось, то корректируем их макс число
    maxParentheses = Math.Min(maxParentheses, availableIndices.Count);

    // выбираем случайные операции для скобок
    List<int> chosenIndices = new List<int>();
    int attempts = 0; // Счетчик попыток
    int maxAttempts = availableIndices.Count * 2; // лимит попыток

    while (chosenIndices.Count < maxParentheses && attempts < maxAttempts)
    {
      int randomIndex = availableIndices[Rand.Next(availableIndices.Count)];

      // проверяем, что выбранная операция не создаст две подряд скобки
      if (!chosenIndices.Contains(randomIndex) &&
          !chosenIndices.Contains(randomIndex - 1) &&
          !chosenIndices.Contains(randomIndex + 1))
      {
        chosenIndices.Add(randomIndex);
      }

      attempts++;
    }

    // устанавливаем true для выбранных операций
    foreach (int index in chosenIndices)
      parentheses[index] = true;

    return parentheses;
  }

  // определение порядка выполнения операций
  private int[] GetOperationOrder(string[] signs, bool[] parentheses)
  {
    List<(int index, int priority)> operations = new List<(int, int)>();

    for (int i = 0; i < signs.Length; i++)
    {
      int priority = 0;

      // операции внутри скобок имеют наивысший приоритет
      if (parentheses[i])
        priority += 2;

      // * и / имеют средний приоритет
      if (signs[i] == "*" || signs[i] == "/")
        priority += 1;

      operations.Add((i, priority));
    }

    // сортируем по убыванию приоритета
    var sortedOperations = operations.OrderByDescending(op => op.priority).ThenBy(op => op.index);

    // заполняем массив
    int[] order = sortedOperations.Select(op => op.index).ToArray();

    return order;
  }

  // генерация чисел по порядку приоритетов
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
      // если оба числа есть, ничего не делаем

      // Выполняем операцию между числами
      int result = PerformOperation(left.Value, right.Value, signs[operationIndex]);

      // т.к. можем получить плохие числа
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

  // генерация второго числа
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

  // Метод для выполнения операции
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
          return int.MinValue; // спец значение ошибки
        return left / right;
      default:
        throw new ArgumentException("Неподдерживаемая операция");
    }
  }

  // обновляем массив промежуточных значений
  private void UpdateIntermediateArray(int?[] intermediateValues, int index, int newValue)
  {
    intermediateValues[index] = newValue;

    for (int i = index - 1; i >= 0 && intermediateValues[i] != null; i--)
      intermediateValues[i] = newValue;

    for (int i = index + 1; i < intermediateValues.Length && intermediateValues[i] != null; i++)
      intermediateValues[i] = newValue;
  }

  // собираем строку
  private string BuildExpression(int?[] numbers, string[] signs, bool[] parentheses)
  {
    // вряд ли где-то будет ошибка, но пускай
    if (numbers.Length != signs.Length + 1 || signs.Length != parentheses.Length)
      throw new ArgumentException("Несоответствие длин массивов numbers, signs и parentheses.");

    StringBuilder expression = new StringBuilder();

    // первое число отдельно прибавляем
    if (parentheses[0])
      expression.Append($"({numbers[0]}");
    else
      expression.Append($"{numbers[0]}");

    // остальные числа и знаки
    for (int i = 0; i < signs.Length; i++)
    {
      if (parentheses[i]) // если эта операция в скобке
        expression.Append($" {signs[i]} {numbers[i + 1]})");
      else if (i + 1 < parentheses.Length && parentheses[i + 1]) // иначе если следующая операция в скобках
        expression.Append($" {signs[i]} ({numbers[i + 1]}");
      else // иначе нет скобок
        expression.Append($" {signs[i]} {numbers[i + 1]}");
    }

    return expression.ToString();
  }

  // прячём элемент
  private (string hiddenExpression, string hiddenElement, bool isNumber) HideRandomElement(string expression, int?[] numbers, string[] signs)
  {
    // числа в строку
    string[] numberStrings = numbers
        .Where(n => n.HasValue)
        .Select(n => n.Value.ToString())
        .ToArray();

    // сначала либо числа, либо знаки
    string[][] targetSets = Rand.Next(2) == 0
        ? new string[][] { numberStrings, signs }
        : new string[][] { signs, numberStrings };

    // смотрим конкретный массив (вообще, всё должно ограничится первой итерацией и можно без него, но мало ли)
    for (int i = 0; i < targetSets.Length; i++)
    {
      List<(int Index, string Value)> matches = new List<(int, string)>();

      for (int j = 0; j < targetSets[i].Length; j++)
      {
        // регулярка, чтобы найти точные совпадения
        string pattern = ReferenceEquals(targetSets[i], numberStrings)
          ? $@"\b{Regex.Escape(targetSets[i][j])}\b"   // для чисел
          : $@" {Regex.Escape(targetSets[i][j])} ";    // для знаков

        MatchCollection foundMatches = Regex.Matches(expression, pattern);

        foreach (Match match in foundMatches.Cast<Match>())
        {
          matches.Add((match.Index, match.Value));
        }
      }

      // если ни одного совпадения, то повторим для всё заново для второго массива
      if (matches.Count > 0)
      {
        // проверяем что текущий массив совпадает с массивом чисел
        bool isNumber = ReferenceEquals(targetSets[i], numberStrings);
        var selected = matches[Rand.Next(matches.Count)];

        string hiddenElement = selected.Value.Trim();
        string hiddenExpression;

        // если число
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

          // Список знаков, которые проверим
          string[] possibleSigns = { "+", "-", "*", "/" };

          // Список подходящих знаков
          List<string> matchingSigns = new List<string>();

          // Используем StringBuilder для работы с строками
          StringBuilder sb = new StringBuilder(expression);

          // Проверяем каждый знак
          foreach (var sign in possibleSigns)
          {
            sb[selected.Index + 1] = sign[0];

            string modifiedExpression = sb.ToString();
            string resultWithSign = new DataTable().Compute(modifiedExpression, "").ToString();

            // Если результат совпадает с исходным, этот знак подходит
            if (resultWithSign == originalResult)
            {
              matchingSigns.Add(sign);
            }
          }

          if (matchingSigns.Count > 0)
          {
            hiddenExpression = expression.Substring(0, selected.Index) +
                                  " ? " +  // Место для подстановки знака
                                  expression.Substring(selected.Index + selected.Value.Length);

            return (hiddenExpression, string.Join(", ", matchingSigns), isNumber);
          }
        }
      }
    }

    // если в обоих случаях не смогли скрыть элемент
    throw new ArgumentException("Скрыть элемент не удалось. Нет совпадений.");
  }

  // получаем правый операнд (с учётом скобок)
  private static string GetRightOperand(string expression, int startIndex)
  {
    // получаем правцю часть выражения
    expression = expression.Substring(startIndex + 1).Trim();

    // ищем первое число или скобку
    Match match = Regex.Match(expression, @"^\(?\d+(\s*[-+*/]\s*\d+)*\)?");

    if (match.Success)
    {
      return match.Value;
    }

    throw new Exception("Не удалось определить правый операнд.");
  }

  // сложение
  private int GenerateAddition(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    // если левый операнд 25 или больше, правый операнд должен быть 1
    if (leftOperand >= _maxPositiveValue)
      return 1;

    // максимально возможное значение для правого операнда
    int maxPossible = _maxPositiveValue - leftOperand;

    return Rand.Next(1, maxPossible + 1);
  }

  // вычитание (0 используем только в исключении)
  private int GenerateSubtraction(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    if (leftOperand <= 1)
      return 0; // иначе отрицательное получим или 0

    // минимально возможное значение для правого операнда
    int minValue = Math.Max(1, leftOperand - _maxPositiveValue);

    return Rand.Next(minValue, leftOperand);
  }

  // умножение
  private int GenerateMultiplier(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    // (или разрешить умножение влпоть до maxPositiveValue (если 1 * maxPositiveValue, например))
    int maxMultiplication = _maxPositiveValue / leftOperand;

    if (maxMultiplication <= 2)
      return 1;

    return Rand.Next(2, maxMultiplication + 1);
  }

  // деление
  private int GenerateDivision(int leftOperand)
  {
    if (leftOperand < 0)
      throw new ArgumentException("Левый операнд должен быть положительным.");

    // (или разрешить деление влпоть до leftOperand (если 100 / 25, 50, например))
    int maxDivision = _maxPositiveValue / 2;

    // если 0, берём любое число от 1 до maxPositiveValue / 2
    if (leftOperand == 0)
      return Rand.Next(2, maxDivision + 1); // сменил на 2 с 1

    if (leftOperand == 1)
      return 1; // 1 делим только на 1

    List<int> divisors = new List<int>();

    // ищем делители от 2 до maxDivision
    for (int i = 2; i <= maxDivision; i++)
      if (leftOperand % i == 0)
        divisors.Add(i);

    return divisors.Count > 0 ? divisors[Rand.Next(divisors.Count)] : leftOperand;
  }
}
