using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GamesData", menuName = "ScriptableObject/GamesData")]
public class GamesData : ScriptableObject
{
  /// <summary> Данные игры </summary>
  [System.Serializable]
  public class Game
  {
    /// <summary> Название игры для приложения </summary>
    public string GameId;

    /// <summary> Название игры для игрока </summary>
    public string NameGame;

    /// <summary> Название сцены игры </summary>
    public string GameScene;
  }

  /// <summary> Список игр в категории </summary>
  [System.Serializable]
  public class Category
  {
    /// <summary> Категория игры для приложения </summary>
    public string CategoryId;

    /// <summary> Категория игры для игрока </summary>
    public string NameCategory;

    /// <summary> Список игр в категории </summary>
    public Game[] Games;
  }

  /// <summary> Список категорий с играми </summary>
  public Category[] Categories;
}
