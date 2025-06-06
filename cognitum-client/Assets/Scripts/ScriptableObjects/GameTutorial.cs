using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameTutorial", menuName = "ScriptableObject/GameTutorial")]
public class GameTutorial : ScriptableObject
{
  public string GameId;

  public List<TutorialPageData> Pages;
}

[System.Serializable]
public class TutorialPageData
{
  public Sprite Image;

  [TextArea(3, 10)]
  public string Description;
}
