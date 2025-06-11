using UnityEngine;

/// <summary>
/// Инициализирует категории с играми из ScriptableObject <br/>
/// Требуется запускать до перехода в основное меню
/// </summary>
public class GameDataInitializer : MonoBehaviour
{
  [SerializeField] private GamesData _gamesData;

  private void Awake()
  {
    if (!GameDataManager.IsInitialized)
    {
      GameDataManager.Initialize(_gamesData);
    }
  }
}
