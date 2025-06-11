using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
  [SerializeField] private Slider levelSlider;
  [SerializeField] private TMP_Text levelText;

  private GameConfig _gameConfig;

  private int _selectedLevel;

  /// <summary>
  /// Инициализация слайдера уровня на основе конфигурации игры
  /// </summary>
  public void Init(string gameId)
  {
    _gameConfig = GameConfigManager.GetGameConfig(gameId);

    levelSlider.minValue = 1;
    levelSlider.maxValue = _gameConfig.MaxLevelReached;

    _selectedLevel = _gameConfig.CurrentLevel;
    levelSlider.value = _selectedLevel;

    UpdateLevelText(_selectedLevel);

    levelSlider.onValueChanged.AddListener(OnSliderChanged);
  }

  private void OnSliderChanged(float value)
  {
    _selectedLevel = Mathf.RoundToInt(value);
    levelSlider.value = _selectedLevel;
    UpdateLevelText(_selectedLevel);
  }

  private void UpdateLevelText(int level)
  {
    levelText.text = $"Выбранный уровень: {level}";
  }
  public void SaveSelectedLevel()
  {
    _gameConfig.CurrentLevel = _selectedLevel;
  }
}
