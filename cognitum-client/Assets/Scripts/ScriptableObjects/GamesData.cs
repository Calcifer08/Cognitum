using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GamesData", menuName = "ScriptableObject/GamesData")]
public class GamesData : ScriptableObject
{
  /// <summary> ������ ���� </summary>
  [System.Serializable]
  public class Game
  {
    /// <summary> �������� ���� ��� ���������� </summary>
    public string GameId;

    /// <summary> �������� ���� ��� ������ </summary>
    public string NameGame;

    /// <summary> �������� ����� ���� </summary>
    public string GameScene;
  }

  /// <summary> ������ ��� � ��������� </summary>
  [System.Serializable]
  public class Category
  {
    /// <summary> ��������� ���� ��� ���������� </summary>
    public string CategoryId;

    /// <summary> ��������� ���� ��� ������ </summary>
    public string NameCategory;

    /// <summary> ������ ��� � ��������� </summary>
    public Game[] Games;
  }

  /// <summary> ������ ��������� � ������ </summary>
  public Category[] Categories;
}
