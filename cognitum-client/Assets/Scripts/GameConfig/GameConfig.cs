using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// данные игры
[System.Serializable]
public class GameConfig
{
  public int CurrentLevel = 1;
  public int MaxLevelReached = 1;
  public int CountGame = 0; // чтоб задавать id игре // т.е. значит какая игра по счёту, как минимум, нужно для сидов
  public int MaxScore = 0;
}

// список игр
[System.Serializable]
public class GameConfigData
{
  public Dictionary<string, GameConfig> GamesConfigData = new Dictionary<string, GameConfig>();
}
