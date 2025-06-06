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
    // Получаем конфигурацию игры
    _gameConfig = GameConfigManager.GetGameConfig(gameId);

    // Ограничиваем параметры
    levelSlider.minValue = 1;
    levelSlider.maxValue = _gameConfig.MaxLevelReached;

    // Устанавливаем ползунок на текущий уровень
    _selectedLevel = _gameConfig.CurrentLevel;
    levelSlider.value = _selectedLevel;

    // Обновляем текст
    UpdateLevelText(_selectedLevel);

    // Подписываемся на изменение значения
    levelSlider.onValueChanged.AddListener(OnSliderChanged);
  }

  /// <summary>
  /// Обновляет отображаемый уровень и проверяет ограничения
  /// </summary>
  private void OnSliderChanged(float value)
  {
    // Ограничиваем значение до _maxLevelReached
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
