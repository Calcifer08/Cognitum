using System.Collections.Generic;

// файл сыгранных игр

/// <summary> Хранит данные по конкретной игровой сессии </summary>
[System.Serializable]
public class GameSession
{
  public int Score;
  public int Level;
}

/// <summary> Класс для хранения всех игровых сесиий, которые не агрегированы </summary>
[System.Serializable]
public class SessionsData
{

  /// <summary> игры -> даты -> данные </summary>
  public Dictionary<string, Dictionary<string, List<GameSession>>> Sessions = new Dictionary<string, Dictionary<string, List<GameSession>>>();
}

