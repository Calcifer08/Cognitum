using UnityEngine;

public static class NotificationManager
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _notificationClass;


  private static AndroidJavaClass GetNotificationClass()
  {
    if (_notificationClass == null)
    {
      try
      {
        _notificationClass = new AndroidJavaClass("com.example.notifyforunity.NotificationBridge");
        Debug.Log("_notificationClass успешно создан.");
      }
      catch (System.Exception e)
      {
        Debug.LogError($"Ошибка загрузки _notificationClass: {e.Message}");
        return null;
      }
    }
    return _notificationClass;
  }

  /// <summary>
  /// Включает ежедневные уведомления на заданное время с заголовком и сообщением
  /// </summary>
  public static void EnableNotifications(int hour, int minute, string title, string message)
  {
    GetNotificationClass();
    _notificationClass.CallStatic("scheduleNotification", hour, minute, title, message);
    Debug.Log($"[NotificationManager] Уведомления включены на {hour:D2}:{minute:D2} с текстом: {title} - {message}");
  }

  /// <summary>
  /// Отключает уведомления
  /// </summary>
  public static void DisableNotifications()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("cancelNotification");
    Debug.Log("[NotificationManager] Уведомления отключены");
  }

  /// <summary>
  /// Обновляет время последнего захода
  /// </summary>
  public static void UpdateLastVisitTime()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("updateLastVisitTime");
    Debug.Log("[NotificationManager] Время последнего визита обновлено");
  }

  /// <summary>
  /// Получает сохранённое время уведомления (часы и минуты)
  /// </summary>
  public static (int hour, int minute) GetScheduledTime()
  {
    GetNotificationClass();
    int[] result = _notificationClass.CallStatic<int[]>("getScheduledTime");

    if (result != null && result.Length == 2)
    {
      Debug.Log($"[NotificationManager] Установленное время уведомления: {result[0]:D2}:{result[1]:D2}");
      return (result[0], result[1]);
    }

    Debug.LogWarning("[NotificationManager] Не удалось получить время уведомления");
    return (18, 0);
  }


  /// <summary>
  /// Проверяет, есть ли разрешение на уведомления (true — есть)
  /// </summary>
  public static bool HasNotificationPermission()
  {
    GetNotificationClass();
    bool has = _notificationClass.CallStatic<bool>("hasPermission");
    Debug.Log($"[NotificationManager] Разрешение на уведомления: {has}");
    return has;
  }

  /// <summary>
  /// Запрашивает разрешение на уведомления, если его ещё нет (показывает системный диалог)
  /// </summary>
  public static void RequestNotificationPermissionIfNeeded()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("requestPermissionIfNeeded");
    Debug.Log("[NotificationManager] Запрошено разрешение на уведомления");
  }

  /// <summary>
  /// Открывает настройки уведомлений для текущего приложения
  /// </summary>
  public static void OpenNotificationSettings()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("openSettings");
    Debug.Log("[NotificationManager] Открыты настройки уведомлений");
  }

#else

  public static void EnableNotifications(int hour, int minute, string title, string message)
  {
    Debug.Log($"[NotificationManager] (заглушка) Уведомления включены на {hour:D2}:{minute:D2} с текстом: {title} - {message}");
  }

  public static void DisableNotifications()
  {
    Debug.Log("[NotificationManager] (заглушка) Уведомления отключены");
  }

  public static void UpdateLastVisitTime()
  {
    Debug.Log("[NotificationManager] (заглушка) Время последнего визита обновлено");
  }

  public static (int hour, int minute) GetScheduledTime()
  {
    Debug.Log("[NotificationManager] (заглушка) Время уведомления");
    return (18, 0);
  }


  public static bool HasNotificationPermission()
  {
    Debug.Log("[NotificationManager] (заглушка) Проверка разрешений — всегда true");
    return true;
  }

  public static void RequestNotificationPermissionIfNeeded()
  {
    Debug.Log("[NotificationManager] (заглушка) Разрешение запрошено (симулировано)");
  }

  public static void OpenNotificationSettings()
  {
    Debug.Log("[NotificationManager] (заглушка) Открытие настроек уведомлений");
  }
#endif
}
