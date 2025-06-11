using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// ������� ����� ��� ���. <br/>
/// �������� ��������� ���� � ��������: <br/>
/// - ���������� �� ����: NameGame; VersionGame; TimeSession. <br/>
/// - ��������� ����: MaxLevel; StreakCorrectQuestionsToLevelUp; CountMistakesToLevelDown; CountAnswersToQuestion; 
/// CountMistakesForQuestion; TimeMemorizePhase; TimeForgetPhase; TimeAnswerPhase; ResultDisplayTime <br/>
/// - ������� ����: _gameZone; _gameZoneCanvasGroup. <br/>
/// - ���������� � ������� � ������: TextQuestion; _textAnswer. <br/>
/// - ��������� �������� ����� ��� ����: Rand. <br/><br/>
/// 
/// ������������ � ������������� � Awake ��� ���������� ����: <br/>
/// - ��� ���� ���������� �� ����, ������� ����, MaxLevel. <br/>
/// - � Awake ������� InitGameMetadata(), InitGameObject(), InitGameParams() ��� ������������� �����. <br/><br/>
/// 
/// ���� � ������� ������������� � Awake ��� ���������� � �������� ����: <br/>
/// - StreakCorrectQuestionsToLevelUp, CountMistakesToLevelDown, CountAnswersToQuestion,CountMistakesForQuestion,
/// TimeMemorizePhase, TimeForgetPhase, TimeAnswerPhase, ResultDisplayTime <br/>
/// - � Awake ������� InitOptionalGameParams() ��� ��������� ������ ��������. <br/><br/>
/// 
/// ��� ��������� ������� ����������� ���, �������� _frequencyOfControlGames. <br/><br/>
/// 
/// ��� ������������� ����������, ���������� �� ������ ��� ������� - 
/// ���������� ������������� ������� �� � ������� ������ GetSpecificData()
/// </summary>
public abstract class AbstractGameBuilder : MonoBehaviour
{
  /// <summary> ������������ ��� ��������� ����. </summary>
  protected Random Rand { get; private set; }

  // ���� ��� ��������� ����� ��� ���������� ��������� ����� ����
  public int BaseSeed { get; protected set; }
  protected int _seedForLevel;
  protected List<int> _levelsVisited = new List<int>();
  protected bool _isControlGame = false;

  /// <summary> ������� ����������� ���.<br/> ��� ������������� ���������� � Awake. </summary>
  protected int _frequencyOfControlGames = 10;



  // ��� �������� ������������ ������� ���������� � ������ ����.
  /// <summary> ��������� ���������� � Awake.<br/>��������: "MatrixMemory"</summary>
  public string NameGame { get; protected set; }

  /// <summary> ��������� ���������� � Awake.<br/>��������: "1.0.0"</summary>
  public string VersionGame { get; protected set; }

  /// <summary> ��������� ���������� � Awake.<br/>��������: 60f</summary>
  public float TimeSession { get; protected set; }




  /// <summary> ��� ��������� � �������� ����. ����� ������� ����� ���������. </summary>
  [SerializeField] protected GameObject _gameZone;

  /// <summary> �������� � Awake �� _gameZone. </summary>
  protected CanvasGroup _gameZoneCanvasGroup;

  /// <summary> ��� ���� ���������. ����� ������� ����� ���������. </summary>
  [SerializeField] protected GameObject _forgetObject;




  /// <summary> ����������� ���������� ������� <br/>
  /// ��������� ���������� � Awake. </summary>
  public int MaxLevel { get; protected set; } = 1;

  /// <summary> ���������� ���������� �������� ������ ��� ��������� ������ <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public int StreakCorrectQuestionsToLevelUp { get; protected set; } = 1;

  /// <summary> ���������� ������ ��� ��������� ������ <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public int CountMistakesToLevelDown { get; protected set; } = 1;

  /// <summary> ��������� ���������� ������� �� ������ <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public int CountAnswersToQuestion { get; protected set; } = 1;

  /// <summary> ���������� ���������� ������ � ������� <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public int CountMistakesForQuestion { get; protected set; } = 1;

  /// <summary> ����� �� ����������� <br/>
  /// 0 == ����������� ����� �� ����������� <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public float TimeMemorizePhase { get; protected set; } = 1f;

  /// <summary> ���������� ���������� ������ � ������� <br/>
  /// 0 == ���������� ���� ��������� <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public float TimeForgetPhase { get; protected set; } = 1f;

  /// <summary> ����� ��� ������ �� ������ <br/>
  /// /// 0 == ����������� ����� �� ����� <br/>
  /// ����� ��������� ��� ���������� � Awake. </summary>
  public float TimeAnswerPhase { get; protected set; } = 0f;

  /// <summary> ����� ����������� ���������� ������ ����� ���������� �������.
  /// ����� ��������� ��� ���������� � Awake </summary>
  public float ResultDisplayTime { get; protected set; } = 1f;



  /// <summary> ��������������� ��� ������ ��������� �������. </summary>
  public string TextQuestion { get; protected set; }

  /// <summary> ��������������� ��� ������ ��������� �������. </summary>
  public string TextAnswer { get; protected set; }

  /// <summary> ������� ����� �����������.</summary>
  public event Action<AnswerData> OnSetAnswer;



  /// <summary> ������������� ���������� ����.<br/>������� � Awake. </summary>
  protected virtual void InitGameMetadata(string nameGame, string versionGame, float timeSession)
  {
    if (string.IsNullOrWhiteSpace(nameGame))
      throw new ArgumentException("��� ���� �� ����� ���� ������");

    if (!System.Text.RegularExpressions.Regex.IsMatch(nameGame, @"^[a-zA-Z0-9]+$"))
      throw new ArgumentException("��� ���� ������ ��������� ������ ����� � �����");

    if (string.IsNullOrWhiteSpace(versionGame))
      throw new ArgumentException("������ ���� �� ����� ���� ������");

    if (!System.Text.RegularExpressions.Regex.IsMatch(versionGame, @"^\d+\.\d+\.\d+$"))
      throw new ArgumentException("������ ���� ������ ���� � ������� X.X.X, ��� X � �����, � ����� ��������� ��");

    if (timeSession <= 1f)
      throw new ArgumentException("����� ������ ������ ���� ������ ����� �������");

    NameGame = nameGame;
    VersionGame = versionGame;
    TimeSession = timeSession;
  }

  /// <summary> ������������� ������� ����.<br/>������� � Awake.<br/> 
  /// ��� ��������� �������� ����� ��������� - ���������� ������ �� ����������� </summary>
  protected virtual void InitGameObject()
  {
    if (_gameZone == null)
    {
      throw new ArgumentException("���� _gameZone �� ������");
    }

    _gameZoneCanvasGroup = _gameZone.GetComponent<CanvasGroup>();

    if (_gameZoneCanvasGroup == null)
    {
      throw new ArgumentException("��������� CanvasGroup �� ������ �� ������� _gameZone.");
    }
  }

  /// <summary> ������������� ��������� ����.<br/>������� � Awake. </summary>
  protected virtual void InitGameParams(int maxLevel)
  {
    if (maxLevel < 1)
      throw new ArgumentException("������������ ������� �� ����� ���� ������ 1");

    MaxLevel = maxLevel;
  }

  /// /// <summary> ������������� ������ ��������� ����.<br/>������� � Awake.<br/>
  /// �� ���������� � ������ ��� ������������ ��������� ���� ����������.<br/>
  /// ��� ������������� ��������� ����� ���������� ���������� - �������� ����� ����������� ��������� </summary>
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
        "����� ���������� �������� ��� ��������� ������ �� ����� ���� ������ 1.");

    if (mistakesToLevelDown < 1)
      throw new ArgumentException(nameof(streakCorrectQuestions),
        "����� ������������ ������� ��� ��������� ������ �� ����� ���� ������ 1.");

    if (countAnswers < 1)
      throw new ArgumentOutOfRangeException(nameof(countAnswers), 
        "����� ������� �� ������ �� ����� ���� ������ 1.");

    if (countMistakes < 0)
      throw new ArgumentOutOfRangeException(nameof(countMistakes),
        "����� ���������� ������ � ������� �� ����� ���� �������������.");

    if (timeMemorize < 0)
      throw new ArgumentOutOfRangeException(nameof(timeMemorize), 
        "����� ���� ����������� �� ����� ���� �������������.");

    if (timeForget < 0)
      throw new ArgumentOutOfRangeException(nameof(timeForget),
        "����� ���� ��������� �� ����� ���� �������������.");

    if (timeAnswer < 0)
      throw new ArgumentOutOfRangeException(nameof(timeAnswer), 
        "����� ������� �� ������ �� ����� ���� �������������.");

    if (resultDisplayTime < 0)
      throw new ArgumentException(nameof(streakCorrectQuestions),
        "����� ����������� ������ �� ����� ���� �������������");

    StreakCorrectQuestionsToLevelUp = streakCorrectQuestions;
    CountMistakesToLevelDown = mistakesToLevelDown;
    CountAnswersToQuestion = countAnswers;
    CountMistakesForQuestion = countMistakes;
    TimeMemorizePhase = timeMemorize;
    TimeForgetPhase = timeForget;
    TimeAnswerPhase = timeAnswer;
    ResultDisplayTime = resultDisplayTime;
  }

  /// <summary> ������������� ����� ������� � ���������� ��� ���������. </summary>
  public void GameSetLevel(int level)
  {
    if (_isControlGame)
    {
      CalculateSeedForLevel(level);
    }

    CalculateLevelConfig(level);
  }

  /// <summary> ����������� ���� ����������, ����������� ��� ������. </summary>
  protected abstract void CalculateLevelConfig(int level);

  /// <summary> ���������� ������� ����������� ��� ������ ��� ������� ����������. <br/> 
  /// ��������� ������ string, bool, int, float. <br/> 
  /// ������������ ��� �������� � ��� ������ ��� ������� </summary>
  public abstract Dictionary<string, object> GetSpecificData(bool isLevelData);

  /// <summary> ������ ����� ������ � ���� � ��������� ��� ��������. </summary>
  public abstract void GameUpdateQuestion();

  /// <summary> ��������� ���� �����������. </summary>
  public abstract IEnumerator StartMemorizePhaseCoroutine();

  /// <summary> ��������� ���� ���������. </summary>
  public abstract IEnumerator StartForgetPhaseCoroutine();

  /// <summary> ���������� �� ������ �������. <br/>
  /// ��������� ����� � ���������� � ������� ��������. <br/>
  /// ����������� ������� ����� RaiseOnSetAnswer(answerData); ��� �������� ������. </summary>
  public abstract void SetAnswer(string answer);

  public abstract void TimeAnswerDone();

  protected void RaiseOnSetAnswer(AnswerData answerData)
  {
    OnSetAnswer?.Invoke(answerData);
  }

  /// <summary> ��������� ������� ����� � ������ ��������� ��������� ����� ��� ������������� ���. </summary>
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

  /// <summary> ��������� ���������� ����� ��� ������� ������ ����������� ����, ����� ����������. </summary>
  protected void CalculateSeedForLevel(int level)
  {
    if (_levelsVisited.Contains(level))
    {
      Random rand = new Random();
      _seedForLevel = BaseSeed + rand.Next(0, 10000);
      Debug.Log("������� ��� ��������");
      Debug.Log(_seedForLevel + " ���������");
    }
    else
    {
      _levelsVisited.Add(level);
      _seedForLevel = BaseSeed + level;
      Debug.Log(_seedForLevel + " �����������");
    }

    Rand = new Random(_seedForLevel);
  }


  /// <summary> ������������� ���� ��������������� ������� ���� </summary>
  public void SetInteractableGameZone(bool isInteractable)
  {
    _gameZoneCanvasGroup.interactable = isInteractable;
  }
}


