using UnityEngine;

public static class SecureStorage
{
#if UNITY_ANDROID && !UNITY_EDITOR
  private static AndroidJavaClass secureStorageClass;

  private static AndroidJavaClass GetSecureStorageClass()
  {
    if (secureStorageClass == null)
    {
      try
      {
        secureStorageClass = new AndroidJavaClass("com.example.securestorage.SecureStorage");
        Debug.Log("SecureStorageClass ������� ������.");
      }
      catch (System.Exception e)
      {
        Debug.LogError($"������ �������� SecureStorage: {e.Message}");
        return null;
      }
    }
    return secureStorageClass;
  }

  public static bool SaveData(string key, string value)
  {
    var storage = GetSecureStorageClass();

    if (storage != null)
    {
      try
      {
        storage.CallStatic("SetValue", key, value);
        return true;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"������ ���������� ������ � SecureStorage: {e.Message}");
      }
    }

    return false;
  }

  public static string GetData(string key)
  {
    var storage = GetSecureStorageClass();

    if (storage != null)
    {
      try
      {
        return storage.CallStatic<string>("GetValue", key);
      }
      catch (System.Exception e)
      {
        Debug.LogError($"������ ��������� ������ �� SecureStorage: {e.Message}");
      }
    }

    return null;
  }

  public static bool DeleteData(string key)
  {
    var storage = GetSecureStorageClass();

    if (storage != null)
    {
      try
      {
        storage.CallStatic("DeleteValue", key);
        return true;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"������ �������� ������ � SecureStorage: {e.Message}");
      }
    }

    return false;
  }
#else
  public static bool SaveData(string key, string value)
  {
    try
    {
      string filePath = System.IO.Path.Combine(Application.persistentDataPath, key + ".txt");
      System.IO.File.WriteAllText(filePath, value);
      return true;
    }
    catch (System.Exception e)
    {
      Debug.LogError($"������ ���������� ������ � ����: {e.Message}");
    }
    return false;
  }

  public static string GetData(string key)
  {
    try
    {
      string filePath = System.IO.Path.Combine(Application.persistentDataPath, key + ".txt");
      if (System.IO.File.Exists(filePath))
      {
        return System.IO.File.ReadAllText(filePath);
      }
    }
    catch (System.Exception e)
    {
      Debug.LogError($"������ ��������� ������ �� �����: {e.Message}");
    }

    return null;
  }

  public static bool DeleteData(string key)
  {
    try
    {
      string filePath = System.IO.Path.Combine(Application.persistentDataPath, key + ".txt");
      if (System.IO.File.Exists(filePath))
      {
        System.IO.File.Delete(filePath);
        return true;
      }
    }
    catch (System.Exception e)
    {
      Debug.LogError($"������ �������� ������ �� �����: {e.Message}");
    }

    return false;
  }
#endif
}
