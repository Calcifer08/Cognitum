using UnityEngine;

/// <summary>
/// �������������� ��������� � ������ �� ScriptableObject <br/>
/// ��������� ��������� �� �������� � �������� ����
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
