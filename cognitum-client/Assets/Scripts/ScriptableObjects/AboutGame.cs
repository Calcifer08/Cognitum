using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AboutGame", menuName = "ScriptableObject/AboutGame")]
public class AboutGame : ScriptableObject
{
  public string GameId;

  [TextArea(3, 10)]
  public string Description;

  [TextArea(3, 10)]
  public List<string> AboutPages;
}
