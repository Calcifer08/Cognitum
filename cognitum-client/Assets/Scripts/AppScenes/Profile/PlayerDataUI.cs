using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDataUI : MonoBehaviour
{
  [SerializeField] private TMP_Dropdown _ageDropdown;
  [SerializeField] private TMP_Dropdown _genderDropdown;
  [SerializeField] private TMP_Dropdown _educationDropdown;
  [SerializeField] private TMP_Dropdown _sleepDropdown;
  [SerializeField] private TMP_Dropdown _digitalUsageDropdown;
  [SerializeField] private Button _saveButton;

  private PlayerData _playerData;
  private bool _isProcessing = false;

  private void Start()
  {
    _playerData = PlayerDataManager.GetPlayerData();

    SetupDropdown(_ageDropdown, Enum.GetValues(typeof(PlayerData.AgeGroup)).Cast<PlayerData.AgeGroup>());
    SetupDropdown(_genderDropdown, Enum.GetValues(typeof(PlayerData.Genders)).Cast<PlayerData.Genders>());
    SetupDropdown(_educationDropdown, Enum.GetValues(typeof(PlayerData.EducationLevel)).Cast<PlayerData.EducationLevel>());
    SetupDropdown(_sleepDropdown, Enum.GetValues(typeof(PlayerData.SleepDuration)).Cast<PlayerData.SleepDuration>());
    SetupDropdown(_digitalUsageDropdown, Enum.GetValues(typeof(PlayerData.DigitalUsageHours)).Cast<PlayerData.DigitalUsageHours>());

    SetDropdownValue(_ageDropdown, _playerData.Age);
    SetDropdownValue(_genderDropdown, _playerData.Gender);
    SetDropdownValue(_educationDropdown, _playerData.Education);
    SetDropdownValue(_sleepDropdown, _playerData.Sleep);
    SetDropdownValue(_digitalUsageDropdown, _playerData.DigitalUsage);

    _saveButton.onClick.AddListener(OnSaveButtonClicked);
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
      _playerData.Age = (PlayerData.AgeGroup)_ageDropdown.value;
      _playerData.Gender = (PlayerData.Genders)_genderDropdown.value;
      _playerData.Education = (PlayerData.EducationLevel)_educationDropdown.value;
      _playerData.Sleep = (PlayerData.SleepDuration)_sleepDropdown.value;
      _playerData.DigitalUsage = (PlayerData.DigitalUsageHours)_digitalUsageDropdown.value;

      _isProcessing = true;
      _saveButton.interactable = false;
      AndroidToast.Show("Сохранение...");

      await PlayerDataManager.SavePlayerDataAsync(_playerData, true);

      _isProcessing = false;
      _saveButton.interactable = true;
      AndroidToast.Show("Данные пользователя сохранены!");
      Debug.Log("Данные игрока сохранены");
    }
  }
}
