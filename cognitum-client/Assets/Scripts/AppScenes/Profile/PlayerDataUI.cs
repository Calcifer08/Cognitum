using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDataUI : MonoBehaviour
{
  public TMP_Dropdown AgeDropdown;
  public TMP_Dropdown GenderDropdown;
  public TMP_Dropdown EducationDropdown;
  public TMP_Dropdown SleepDropdown;
  public TMP_Dropdown DigitalUsageDropdown;
  public Button SaveButton; // Кнопка сохранения

  private PlayerData _playerData;
  private bool _isProcessing = false; // Флаг, предотвращающий спам сохранений

  private void Start()
  {
    _playerData = PlayerDataManager.GetPlayerData();

    // Заполняем dropdown'ы значениями
    SetupDropdown(AgeDropdown, Enum.GetValues(typeof(PlayerData.AgeGroup)).Cast<PlayerData.AgeGroup>());
    SetupDropdown(GenderDropdown, Enum.GetValues(typeof(PlayerData.Genders)).Cast<PlayerData.Genders>());
    SetupDropdown(EducationDropdown, Enum.GetValues(typeof(PlayerData.EducationLevel)).Cast<PlayerData.EducationLevel>());
    SetupDropdown(SleepDropdown, Enum.GetValues(typeof(PlayerData.SleepDuration)).Cast<PlayerData.SleepDuration>());
    SetupDropdown(DigitalUsageDropdown, Enum.GetValues(typeof(PlayerData.DigitalUsageHours)).Cast<PlayerData.DigitalUsageHours>());

    // Устанавливаем сохранённые значения
    SetDropdownValue(AgeDropdown, _playerData.Age);
    SetDropdownValue(GenderDropdown, _playerData.Gender);
    SetDropdownValue(EducationDropdown, _playerData.Education);
    SetDropdownValue(SleepDropdown, _playerData.Sleep);
    SetDropdownValue(DigitalUsageDropdown, _playerData.DigitalUsage);

    // Назначаем обработчик нажатия кнопки сохранения
    SaveButton.onClick.AddListener(OnSaveButtonClicked);
  }

  private void SetupDropdown<T>(TMP_Dropdown dropdown, IEnumerable<T> values) where T : Enum
  {
    dropdown.ClearOptions();
    List<string> options = values.Select(v => PlayerData.GetEnumMemberValue(v)).ToList();
    dropdown.AddOptions(options);
  }

  private void SetDropdownValue<T>(TMP_Dropdown dropdown, T value) where T : Enum
  {
    string valueStr = PlayerData.GetEnumMemberValue(value);
    int index = dropdown.options.FindIndex(option => option.text == valueStr);
    if (index >= 0)
    {
      dropdown.value = index;
      dropdown.RefreshShownValue();
    }
  }

  private async void OnSaveButtonClicked()
  {
    if (!_isProcessing)
    {
      // Заполняем объект данными перед сохранением
      _playerData.Age = (PlayerData.AgeGroup)AgeDropdown.value;
      _playerData.Gender = (PlayerData.Genders)GenderDropdown.value;
      _playerData.Education = (PlayerData.EducationLevel)EducationDropdown.value;
      _playerData.Sleep = (PlayerData.SleepDuration)SleepDropdown.value;
      _playerData.DigitalUsage = (PlayerData.DigitalUsageHours)DigitalUsageDropdown.value;

      _isProcessing = true;
      SaveButton.interactable = false; // Блокируем кнопку на время сохранения
      AndroidToast.Show("Сохранение...");

      await PlayerDataManager.SavePlayerDataAsync(_playerData, true);

      _isProcessing = false;
      SaveButton.interactable = true;
      AndroidToast.Show("Данные пользователя сохранены!");
      Debug.Log("Данные игрока сохранены");
    }
  }
}
