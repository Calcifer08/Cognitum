using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public static class AppSettingsManager
{
  public const string FileName = "AppSettings.json";
  private static AppSettings _settings;
  private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);

  public static async Task InitializeAsync()
  {
    if (_settings == null)
    {
      await LoadConfigAsync();


      bool isHasPermission = NotificationManager.HasNotificationPermission();

      if (!isHasPermission)
      {
        NotificationManager.RequestNotificationPermissionIfNeeded();
      }

      bool isEnableNotifications = isHasPermission && _settings.Notifications.Enabled;

      SetNotificationsEnabled(isEnableNotifications);
      NotificationManager.UpdateLastVisitTime();
    }
  }

  private static async Task LoadConfigAsync()
  {
    if (File.Exists(FilePath))
    {
      string json = await File.ReadAllTextAsync(FilePath);
      _settings = JsonConvert.DeserializeObject<AppSettings>(json);
      Debug.Log("Файл настроек загружен");
    }
    else
    {
      _settings = new AppSettings();
      await SaveSettingsAsync();
      Debug.Log("Файл настроек не найден. Создан новый");
    }
  }

  public static AppSettings GetSettings()
  {
    return _settings;
  }

  public static async Task SaveSettingsAsync()
  {
    try
    {
#if !UNITY_EDITOR
    string json = JsonConvert.SerializeObject(_settings, Formatting.None);
#elif UNITY_EDITOR
      string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
#endif

      await File.WriteAllTextAsync(FilePath, json);
      Debug.Log("Файл настроек сохранён");
    }
    catch (Exception ex)
    {
      Debug.LogError("Ошибка при сохранении настроек: " + ex.Message);
    }
  }

  public static void SetSoundEnabled(bool enabled)
  {
    _settings.Sound.Enabled = enabled;
  }

  public static void SetSoundVolume(float volume)
  {
    _settings.Sound.Volume = volume;
  }

  public static bool SetNotificationsEnabled(bool enabled)
  {
    if (enabled)
    {
      if (!NotificationManager.HasNotificationPermission())
      {
        _settings.Notifications.Enabled = false;

        NotificationManager.OpenNotificationSettings();

        return false;
      }

      _settings.Notifications.Enabled = true;
    }
    else
    {
      _settings.Notifications.Enabled = false;
      NotificationManager.DisableNotifications();
    }
    return true;
  }

  public static void SetTimeNotifications(int hour, int minute)
  {
    _settings.Notifications.Hour = hour;
    _settings.Notifications.Minute = minute;

    if (_settings.Notifications.Enabled)
    {
      NotificationManager.EnableNotifications(hour, minute, "Ежедневное уведомление", "Не забудь про тренировку!");
    }
  }

  public static (int hour, int minute) GetTimeNotifications()
  {
    return NotificationManager.GetScheduledTime();
  }
}
