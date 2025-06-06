using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������ ����
[System.Serializable]
public class GameConfig
{
  public int CurrentLevel = 1;
  public int MaxLevelReached = 1;
  public int CountGame = 0; // ���� �������� id ���� // �.�. ������ ����� ���� �� �����, ��� �������, ����� ��� �����
  public int MaxScore = 0;
}

// ������ ���
[System.Serializable]
public class GameConfigData
{
  public Dictionary<string, GameConfig> GamesConfigData = new Dictionary<string, GameConfig>();
}
