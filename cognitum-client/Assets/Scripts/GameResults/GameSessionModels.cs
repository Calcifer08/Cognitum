using System.Collections.Generic;

// SessionsData.json

/// <summary> ������ �� ���������� ������� ������ </summary>
[System.Serializable]
public class GameSession
{
  public int Score;
  public int Level;
}

/// <summary> ����� ��� ����������� ���� ������� ������, ������� �� ������������ </summary>
[System.Serializable]
public class SessionsData
{

  /// <summary> ���� -> ���� -> ������ </summary>
  public Dictionary<string, Dictionary<string, List<GameSession>>> Sessions = new Dictionary<string, Dictionary<string, List<GameSession>>>();
}

