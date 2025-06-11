using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEventSystem = UnityEngine.EventSystems;
using System.Collections.Generic;

public class AppSettingsUI : MonoBehaviour
{
  [SerializeField] private Toggle _soundToggle;
  [SerializeField] private Slider _soundVolumeSlider;
  [SerializeField] private TMP_Text _volumeLabel;

  [SerializeField] private Toggle _notificationToggle;
  [SerializeField] private GameObject _notifyTimeContainer;
  [SerializeField] private TMP_Dropdown _hourDropdown;
  [SerializeField] private TMP_Dropdown _minuteDropdown;

  [SerializeField] private Button _backButton;

  [Header("Отключение в игре")]
  [SerializeField] private UnityEventSystem.EventSystem _eventSystem;
  [SerializeField] private AudioListener _audioListener;

  private AppSettings _settings;

  private void Awake()
  {
    if (SceneManager.sceneCount > 1)
    {
      _eventSystem.gameObject.SetActive(false);
      _audioListener.gameObject.SetActive(false);

      _backButton.gameObject.SetActive(true);
      _backButton.onClick.AddListener(() => SceneManager.UnloadSceneAsync(SceneNames.Settings));
    }
    else
    {
      _eventSystem.gameObject.SetActive(true);
      _audioListener.gameObject.SetActive(true);
    }
  }

  private void Start()
  {
    _settings = AppSettingsManager.GetSettings();

    _soundToggle.isOn = _settings.Sound.Enabled;
    _soundVolumeSlider.value = _settings.Sound.Volume;
    _volumeLabel.text = Mathf.RoundToInt(_settings.Sound.Volume * 100f) + "%";

    _notificationToggle.isOn = _settings.Notifications.Enabled;

    _soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
    _soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
    _notificationToggle.onValueChanged.AddListener(OnNotificationToggleChanged);

    InitTimeDropdownOptions();
    InitTimeDropdowns();

    _hourDropdown.onValueChanged.AddListener(OnTimeDropdownChanged);
    _minuteDropdown.onValueChanged.AddListener(OnTimeDropdownChanged);

    _notifyTimeContainer.SetActive(_notificationToggle.isOn);
  }

  private void InitTimeDropdownOptions()
  {
    if (_hourDropdown != null)
    {
      _hourDropdown.ClearOptions();
      var hourOptions = new List<string>();
      for (int i = 0; i < 24; i++)
        hourOptions.Add(i.ToString("D2"));
      _hourDropdown.AddOptions(hourOptions);
    }

    if (_minuteDropdown != null)
    {
      _minuteDropdown.ClearOptions();
      var minuteOptions = new List<string>();
      for (int i = 0; i < 60; i++)
        minuteOptions.Add(i.ToString("D2"));
      _minuteDropdown.AddOptions(minuteOptions);
    }
  }

  private void InitTimeDropdowns()
  {
    if (_hourDropdown != null)
    {
      _hourDropdown.value = Mathf.Clamp(_settings.Notifications.Hour, 0, _hourDropdown.options.Count - 1);
      _hourDropdown.RefreshShownValue();
    }

    if (_minuteDropdown != null)
    {
      _minuteDropdown.value = Mathf.Clamp(_settings.Notifications.Minute, 0, _minuteDropdown.options.Count - 1);
      _minuteDropdown.RefreshShownValue();
    }
  }

  private void OnSoundToggleChanged(bool isOn)
  {
    AppSettingsManager.SetSoundEnabled(isOn);
  }

  private void OnSoundVolumeChanged(float value)
  {
    value = (float)System.Math.Round(value, 2);
    AppSettingsManager.SetSoundVolume(value);
    _volumeLabel.text = value * 100f + "%";
  }

  private void OnNotificationToggleChanged(bool isOn)
  {
    bool isHasPermission = AppSettingsManager.SetNotificationsEnabled(isOn);

    if (!isHasPermission)
    {
      isOn = false;
      _notificationToggle.isOn = isOn;
    }

    _notifyTimeContainer.SetActive(isOn);

    if (isOn)
      OnTimeDropdownChanged(0);
  }

  private void OnTimeDropdownChanged(int _)
  {
    if (_notificationToggle.isOn)
    {
      var hour = _hourDropdown.value;
      var minute = _minuteDropdown.value;

      AppSettingsManager.SetTimeNotifications(hour, minute);
    }
  }

  private void OnDestroy()
  {
    _ = AppSettingsManager.SaveSettingsAsync();
  }
}
