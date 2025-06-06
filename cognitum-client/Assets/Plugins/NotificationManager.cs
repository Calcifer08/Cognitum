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
        Debug.Log("_notificationClass ������� ������.");
      }
      catch (System.Exception e)
      {
        Debug.LogError($"������ �������� _notificationClass: {e.Message}");
        return null;
      }
    }
    return _notificationClass;
  }

  /// <summary>
  /// �������� ���������� ����������� �� �������� ����� � ���������� � ����������
  /// </summary>
  public static void EnableNotifications(int hour, int minute, string title, string message)
  {
    GetNotificationClass();
    _notificationClass.CallStatic("scheduleNotification", hour, minute, title, message);
    Debug.Log($"[NotificationManager] ����������� �������� �� {hour:D2}:{minute:D2} � �������: {title} - {message}");
  }

  /// <summary>
  /// ��������� �����������
  /// </summary>
  public static void DisableNotifications()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("cancelNotification");
    Debug.Log("[NotificationManager] ����������� ���������");
  }

  /// <summary>
  /// ��������� ����� ���������� ������
  /// </summary>
  public static void UpdateLastVisitTime()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("updateLastVisitTime");
    Debug.Log("[NotificationManager] ����� ���������� ������ ���������");
  }

  /// <summary>
  /// �������� ���������� ����� ����������� (���� � ������)
  /// </summary>
  public static (int hour, int minute) GetScheduledTime()
  {
    GetNotificationClass();
    int[] result = _notificationClass.CallStatic<int[]>("getScheduledTime");

    if (result != null && result.Length == 2)
    {
      Debug.Log($"[NotificationManager] ������������� ����� �����������: {result[0]:D2}:{result[1]:D2}");
      return (result[0], result[1]);
    }

    Debug.LogWarning("[NotificationManager] �� ������� �������� ����� �����������");
    return (18, 0);
  }


  /// <summary>
  /// ���������, ���� �� ���������� �� ����������� (true � ����)
  /// </summary>
  public static bool HasNotificationPermission()
  {
    GetNotificationClass();
    bool has = _notificationClass.CallStatic<bool>("hasPermission");
    Debug.Log($"[NotificationManager] ���������� �� �����������: {has}");
    return has;
  }

  /// <summary>
  /// ����������� ���������� �� �����������, ���� ��� ��� ��� (���������� ��������� ������)
  /// </summary>
  public static void RequestNotificationPermissionIfNeeded()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("requestPermissionIfNeeded");
    Debug.Log("[NotificationManager] ��������� ���������� �� �����������");
  }

  /// <summary>
  /// ��������� ��������� ����������� ��� �������� ����������
  /// </summary>
  public static void OpenNotificationSettings()
  {
    GetNotificationClass();
    _notificationClass.CallStatic("openSettings");
    Debug.Log("[NotificationManager] ������� ��������� �����������");
  }

#else

  public static void EnableNotifications(int hour, int minute, string title, string message)
  {
    Debug.Log($"[NotificationManager] (��������) ����������� �������� �� {hour:D2}:{minute:D2} � �������: {title} - {message}");
  }

  public static void DisableNotifications()
  {
    Debug.Log("[NotificationManager] (��������) ����������� ���������");
  }

  public static void UpdateLastVisitTime()
  {
    Debug.Log("[NotificationManager] (��������) ����� ���������� ������ ���������");
  }

  public static (int hour, int minute) GetScheduledTime()
  {
    Debug.Log("[NotificationManager] (��������) ����� �����������");
    return (18, 0);
  }


  public static bool HasNotificationPermission()
  {
    Debug.Log("[NotificationManager] (��������) �������� ���������� � ������ true");
    return true;
  }

  public static void RequestNotificationPermissionIfNeeded()
  {
    Debug.Log("[NotificationManager] (��������) ���������� ��������� (������������)");
  }

  public static void OpenNotificationSettings()
  {
    Debug.Log("[NotificationManager] (��������) �������� �������� �����������");
  }
#endif
}
