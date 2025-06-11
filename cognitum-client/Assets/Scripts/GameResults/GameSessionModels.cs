using System.Collections.Generic;

// SessionsData.json

/// <summary> Данные по конкретной игровой сессии </summary>
[System.Serializable]
public class GameSession
{
  public int Score;
  public int Level;
}

/// <summary> Класс для результатов всех игровых сесиий, которые не агрегированы </summary>
[System.Serializable]
public class SessionsData
{

  /// <summary> игры -> даты -> данные </summary>
  public Dictionary<string, Dictionary<string, List<GameSession>>> Sessions = new Dictionary<string, Dictionary<string, List<GameSession>>>();
}

