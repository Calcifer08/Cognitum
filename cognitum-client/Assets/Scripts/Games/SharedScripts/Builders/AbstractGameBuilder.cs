using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Базовый класс для игр. <br/>
/// Содержит следующие поля и свойства: <br/>
/// - Информация по игре: NameGame; VersionGame; TimeSession. <br/>
/// - Настройки игры: MaxLevel; StreakCorrectQuestionsToLevelUp; CountMistakesToLevelDown; CountAnswersToQuestion; 
/// CountMistakesForQuestion; TimeMemorizePhase; TimeForgetPhase; TimeAnswerPhase; ResultDisplayTime <br/>
/// - Объекты игры: _gameZone; _gameZoneCanvasGroup. <br/>
/// - Информация о вопросе и ответе: TextQuestion; _textAnswer. <br/>
/// - Генератор случаных чисел для игры: Rand. <br/><br/>
/// 
/// Обязательные к инициализации в Awake или инспекторе поля: <br/>
/// - Все поля информации по игре, объекты игры, MaxLevel. <br/>
/// - В Awake вызвать InitGameMetadata(), InitGameObject(), InitGameParams() для инициализации полей. <br/><br/>
/// 
/// Поля с выбором инициализиции в Awake или изменяемые в процессе игры: <br/>
/// - StreakCorrectQuestionsToLevelUp, CountMistakesToLevelDown, CountAnswersToQuestion,CountMistakesForQuestion,
/// TimeMemorizePhase, TimeForgetPhase, TimeAnswerPhase, ResultDisplayTime <br/>
/// - В Awake вызвать InitOptionalGameParams() для установки нужных значений. <br/><br/>
/// 
/// Для изменения частоты контрольных игр, изменить _frequencyOfControlGames. <br/><br/>
/// 
/// При использовании параметров, меняющихся от уровня или вопроса - 
/// необходимо дополнительно указать их в словаре метода GetSpecificData()
/// </summary>
public abstract class AbstractGameBuilder : MonoBehaviour
{
  /// <summary> Использовать для генерации игры. </summary>
  protected Random Rand { get; private set; }

  // Поля для генерации зерна для генератора случайных чисел игры
  public int BaseSeed { get; protected set; }
  protected int _seedForLevel;
  protected List<int> _levelsVisited = new List<int>();
  protected bool _isControlGame = false;

  /// <summary> Частота контрольных игр.<br/> При необходимости установить в Awake. </summary>
  protected int _frequencyOfControlGames = 10;



  // Эти значения используются игровым менеджером в начале игры.
  /// <summary> Требуется установить в Awake.<br/>Например: "MatrixMemory"</summary>
  public string NameGame { get; protected set; }

  /// <summary> Требуется установить в Awake.<br/>Например: "1.0.0"</summary>
  public string VersionGame { get; protected set; }

  /// <summary> Требуется установить в Awake.<br/>Например: 60f</summary>
  public float TimeSession { get; protected set; }




  /// <summary> Для обращения к объектам игры. Можно указать через инспектор. </summary>
  [SerializeField] protected GameObject _gameZone;

  /// <summary> Получаем в Awake из _gameZone. </summary>
  protected CanvasGroup _gameZoneCanvasGroup;

  /// <summary> Для Фазы забывания. Можно указать через инспектор. </summary>
  [SerializeField] protected GameObject _forgetObject;




  /// <summary> Максимально допустимый уровень <br/>
  /// Требуется установить в Awake. </summary>
  public int MaxLevel { get; protected set; } = 1;

  /// <summary> Количество правильных вопросов подряд для повышения уровня <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public int StreakCorrectQuestionsToLevelUp { get; protected set; } = 1;

  /// <summary> Количество ошибок для понижения уровня <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public int CountMistakesToLevelDown { get; protected set; } = 1;

  /// <summary> Требуемое количество ответов на вопрос <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public int CountAnswersToQuestion { get; protected set; } = 1;

  /// <summary> Допустимое количество ошибок в вопросе <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public int CountMistakesForQuestion { get; protected set; } = 1;

  /// <summary> Время на запоминание <br/>
  /// 0 == бесконечное время на запоминание <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public float TimeMemorizePhase { get; protected set; } = 1f;

  /// <summary> Допустимое количество ошибок в вопросе <br/>
  /// 0 == отсутствие фазы забывания <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public float TimeForgetPhase { get; protected set; } = 1f;

  /// <summary> Время для ответа на вопрос <br/>
  /// /// 0 == бесконечное время на ответ <br/>
  /// Можно вычислять или установить в Awake. </summary>
  public float TimeAnswerPhase { get; protected set; } = 0f;

  /// <summary> Время отображения результата ответа перед обновление вопроса.
  /// Можно вычислять или установить в Awake </summary>
  public float ResultDisplayTime { get; protected set; } = 1f;



  /// <summary> Устанавливается при каждой генерации вопроса. </summary>
  public string TextQuestion { get; protected set; }

  /// <summary> Устанавливается при каждой генерации вопроса. </summary>
  public string TextAnswer { get; protected set; }

  /// <summary> Передаёт ответ подписчикам.</summary>
  public event Action<AnswerData> OnSetAnswer;



  /// <summary> Устанавливает метаданные игры.<br/>Вызвать в Awake. </summary>
  protected virtual void InitGameMetadata(string nameGame, string versionGame, float timeSession)
  {
    if (string.IsNullOrWhiteSpace(nameGame))
      throw new ArgumentException("Имя игры не может быть пустым");

    if (!System.Text.RegularExpressions.Regex.IsMatch(nameGame, @"^[a-zA-Z0-9]+$"))
      throw new ArgumentException("Имя игры должно содержать только буквы и цифры");

    if (string.IsNullOrWhiteSpace(versionGame))
      throw new ArgumentException("Версия игры не может быть пустой");

    if (!System.Text.RegularExpressions.Regex.IsMatch(versionGame, @"^\d+\.\d+\.\d+$"))
      throw new ArgumentException("Версия игры должна быть в формате X.X.X, где X — цифры, а точки разделяют их");

    if (timeSession <= 1f)
      throw new ArgumentException("Время сессии должно быть больше одной секунды");

    NameGame = nameGame;
    VersionGame = versionGame;
    TimeSession = timeSession;
  }

  /// <summary> Устанавливает объекты игры.<br/>Вызвать в Awake.<br/> 
  /// При установке объектов через инспектор - передавать ссылки не обязательно </summary>
  protected virtual void InitGameObject()
  {
    if (_gameZone == null)
    {
      throw new ArgumentException("Поле _gameZone не задано");
    }

    _gameZoneCanvasGroup = _gameZone.GetComponent<CanvasGroup>();

    if (_gameZoneCanvasGroup == null)
    {
      throw new ArgumentException("Компонент CanvasGroup не найден на объекте _gameZone.");
    }
  }

  /// <summary> Устанавливает параметры игры.<br/>Вызвать в Awake. </summary>
  protected virtual void InitGameParams(int maxLevel)
  {
    if (maxLevel < 1)
      throw new ArgumentException("Максимальный уровень не может быть меньше 1");

    MaxLevel = maxLevel;
  }

  /// /// <summary> Устанавливает прочие параметры игры.<br/>Вызвать в Awake.<br/>
  /// Не обязателен к вызову при динамическом изменении всех параметров.<br/>
  /// При необходимости установки части постоянных параметров - передать через именованные параметры </summary>
  protected virtual void InitOptionalGameParams(
    int streakCorrectQuestions = 1, 
    int mistakesToLevelDown = 1,
    int countAnswers = 1,
    int countMistakes = 0, 
    float timeMemorize = 0f, 
    float timeForget = 0f, 
    float timeAnswer = 0f,
    float resultDisplayTime = 0f)
  {
    if (streakCorrectQuestions < 1)
      throw new ArgumentException(nameof(streakCorrectQuestions), 
        "Число правильных вопросов для повышения уровня не может быть меньше 1.");

    if (mistakesToLevelDown < 1)
      throw new ArgumentException(nameof(streakCorrectQuestions),
        "Число неправильных ответов для понижения уровня не может быть меньше 1.");

    if (countAnswers < 1)
      throw new ArgumentOutOfRangeException(nameof(countAnswers), 
        "Число ответов на вопрос не может быть меньше 1.");

    if (countMistakes < 0)
      throw new ArgumentOutOfRangeException(nameof(countMistakes),
        "Число допустимых ошибок в вопросе не может быть отрицательным.");

    if (timeMemorize < 0)
      throw new ArgumentOutOfRangeException(nameof(timeMemorize), 
        "Время фазы запоминания не может быть отрицательным.");

    if (timeForget < 0)
      throw new ArgumentOutOfRangeException(nameof(timeForget),
        "Время фазы забывания не может быть отрицательным.");

    if (timeAnswer < 0)
      throw new ArgumentOutOfRangeException(nameof(timeAnswer), 
        "Лимит времени на вопрос не может быть отрицательным.");

    if (resultDisplayTime < 0)
      throw new ArgumentException(nameof(streakCorrectQuestions),
        "Время отображения ответа не может быть отрицательным");

    StreakCorrectQuestionsToLevelUp = streakCorrectQuestions;
    CountMistakesToLevelDown = mistakesToLevelDown;
    CountAnswersToQuestion = countAnswers;
    CountMistakesForQuestion = countMistakes;
    TimeMemorizePhase = timeMemorize;
    TimeForgetPhase = timeForget;
    TimeAnswerPhase = timeAnswer;
    ResultDisplayTime = resultDisplayTime;
  }

  /// <summary> Устанавливает новый уровень и возвращает его параметры. </summary>
  public void GameSetLevel(int level)
  {
    if (_isControlGame)
    {
      CalculateSeedForLevel(level);
    }

    CalculateLevelConfig(level);
  }

  /// <summary> Вычисленные всех переменных, необходимых для уровня. </summary>
  protected abstract void CalculateLevelConfig(int level);

  /// <summary> Возвращает словарь специфичных для уровня или вопроса параметров. <br/> 
  /// Разрешены только string, bool, int, float. <br/> 
  /// Используется для передачи в лог уровня или вопроса </summary>
  public abstract Dictionary<string, object> GetSpecificData(bool isLevelData);

  /// <summary> Создаёт новый вопрос в игре и возращает его значения. </summary>
  public abstract void GameUpdateQuestion();

  /// <summary> Запускает фазу запоминания. </summary>
  public abstract IEnumerator StartMemorizePhaseCoroutine();

  /// <summary> Запускает фазу забывания. </summary>
  public abstract IEnumerator StartForgetPhaseCoroutine();

  /// <summary> Вызывается из кнопок ответов. <br/>
  /// Проверяет ответ и отправляет в игровой менеджер. <br/>
  /// Обязательно сделать вызов RaiseOnSetAnswer(answerData); для передачи ответа. </summary>
  public abstract void SetAnswer(string answer);

  public abstract void TimeAnswerDone();

  protected void RaiseOnSetAnswer(AnswerData answerData)
  {
    OnSetAnswer?.Invoke(answerData);
  }

  /// <summary> Вычисляет базовое зерно и создаёт генератор случайных чисел для неконтрольных игр. </summary>
  public virtual int GetBaseSeedForGame(int countGame)
  {
    using MD5 md5 = MD5.Create();
    string word = NameGame + countGame;
    byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(word));

    BaseSeed = (hashBytes[0] << 16) | (hashBytes[1] << 8) | hashBytes[2];

    if (countGame % _frequencyOfControlGames == 0)
    {
      _isControlGame = true;
    }
    else
    {
      Random rand = new Random();
      BaseSeed += rand.Next(0, 10000);
      Rand = new Random(BaseSeed);
    }

    return BaseSeed;
  }

  /// <summary> Вычисляет уникальное зерно для каждого уровня контрольной игры, кроме посещённых. </summary>
  protected void CalculateSeedForLevel(int level)
  {
    if (_levelsVisited.Contains(level))
    {
      Random rand = new Random();
      _seedForLevel = BaseSeed + rand.Next(0, 10000);
      Debug.Log("Уровень уже посещали");
      Debug.Log(_seedForLevel + " Рандомное");
    }
    else
    {
      _levelsVisited.Add(level);
      _seedForLevel = BaseSeed + level;
      Debug.Log(_seedForLevel + " Контрольное");
    }

    Rand = new Random(_seedForLevel);
  }


  /// <summary> Устанавливает флаг интерактивности игровой зоны </summary>
  public void SetInteractableGameZone(bool isInteractable)
  {
    _gameZoneCanvasGroup.interactable = isInteractable;
  }
}


