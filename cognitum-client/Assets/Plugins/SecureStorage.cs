using UnityEngine;

public static class SecureStorage
{
#if UNITY_ANDROID && !UNITY_EDITOR
  // Для работы с Android
  private static AndroidJavaClass secureStorageClass;

  // Получение класса для SecureStorage в Android
  private static AndroidJavaClass GetSecureStorageClass()
  {
    if (secureStorageClass == null)
    {
      try
      {
        secureStorageClass = new AndroidJavaClass("com.example.securestorage.SecureStorage");
        Debug.Log("SecureStorageClass успешно создан.");
      }
      catch (System.Exception e)
      {
        Debug.LogError($"Ошибка загрузки SecureStorage: {e.Message}");
        return null;
      }
    }
    return secureStorageClass;
  }

  // Метод для сохранения данных
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
        Debug.LogError($"Ошибка сохранения данных в SecureStorage: {e.Message}");
      }
    }

    return false;
  }

  // Метод для получения данных
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
        Debug.LogError($"Ошибка получения данных из SecureStorage: {e.Message}");
      }
    }

    return null;
  }

  // Метод для удаления данных
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
        Debug.LogError($"Ошибка удаления данных в SecureStorage: {e.Message}");
      }
    }

    return false;
  }
#else
  // Для редактора сохраняем данные в папку на ПК
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
      Debug.LogError($"Ошибка сохранения данных в файл: {e.Message}");
    }
    return false;
  }

  // Для редактора получаем данные из файла
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
      Debug.LogError($"Ошибка получения данных из файла: {e.Message}");
    }

    return null;
  }

  // Для редактора удаляем данные из файла
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
      Debug.LogError($"Ошибка удаления данных из файла: {e.Message}");
    }

    return false;
  }
#endif
}
