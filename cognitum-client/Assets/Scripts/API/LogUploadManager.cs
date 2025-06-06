using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// Класс для отправки всех сохранённых логов на сервер.
/// </summary>
public static class LogUploadManager
{
  private static readonly string _logsDirectoryPath = Path.Combine(Application.persistentDataPath, LogSessionManager.Folder);
  private static readonly int _batchSize = 5; // Размер батча
  private static readonly float _delayBetweenBatches = 1f; //  Задержка между отправками батчей

  /// <summary>
  /// Корутина для отправки всех накопленных логов.
  /// </summary>
  /// 
  public static async Task SendAllLogsAsync(int retryCount = 0)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.Log("Нет интернета, логи будут отправлены позже");
      return;
    }

    if (!Directory.Exists(_logsDirectoryPath))
    {
      Debug.LogWarning("Директория с логами не существует. Логи не будут отправлены.");
      return; // Выход из функции, если директория не существует
    }

    var logFiles = Directory.GetFiles(_logsDirectoryPath, "*.json");

    // Если логов нет, то выводим сообщение и выходим
    if (logFiles.Length == 0)
    {
      Debug.Log("Нет логов для отправки");
      return;
    }

    string token = AuthManager.GetAccessToken();
    if (string.IsNullOrEmpty(token))
    {
      Debug.Log("Ошибка: нет токена для отправки логов");
      AuthManager.ClearTokensAndLogout();
      return;
    }

    for (int i = 0; i < logFiles.Length; i += _batchSize)
    {
      // Читаем батч логов
      var batchFiles = logFiles.Skip(i).Take(_batchSize).ToList();
      var batchContents = new List<string>();

      foreach (var file in batchFiles)
      {
        string content = await File.ReadAllTextAsync(file);
        batchContents.Add(content);
      }

      // Объединяем JSON-строки в массив
      string batchJson = $"[{string.Join(",", batchContents)}]";

      using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.SaveLogsUrl, "POST"))
      {
        byte[] postData = Encoding.UTF8.GetBytes(batchJson);
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.SetRequestHeader("Content-Type", "application/json");

        // Ожидаем завершения запроса асинхронно
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
          await Task.Yield();  // Освобождаем поток для других задач
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
#if !UNITY_EDITOR
          // Удаляем файлы сразу после отправки батча
          foreach (var file in batchFiles)
          {
            File.Delete(file);
          }
#else
          string deleteLogsFolder = Path.Combine(_logsDirectoryPath, "DeleteLogs");
          if (!Directory.Exists(deleteLogsFolder))
          {
            Directory.CreateDirectory(deleteLogsFolder);
          }

          // Перемещаем файлы в папку DeleteLogs
          foreach (var file in batchFiles)
          {
            string fileName = Path.GetFileName(file);
            string newFilePath = Path.Combine(deleteLogsFolder, fileName);

            try
            {
              File.Move(file, newFilePath); // Перемещаем файл
              Debug.Log($"Файл {fileName} перемещен в папку DeleteLogs.");
            }
            catch (Exception ex)
            {
              Debug.LogError($"Не удалось переместить файл {fileName}: {ex.Message}");
            }
          }
#endif
          Debug.Log($"Отправлено {i + batchContents.Count} из {logFiles.Length} логов");
        }
        else
        {
          await HandleLogErrorResponseAsync(request, retryCount);
          return;
        }
      }

      // Добавляем задержку между батчами
      await Task.Delay((int)(_delayBetweenBatches * 1000)); // Преобразуем задержку в миллисекунды
    }

    Debug.Log("Все логи успешно отправлены и удалены");
  }

  /// <summary>
  /// Обрабатывает ошибки, полученные при отправке логов.
  /// </summary>
  private static async Task HandleLogErrorResponseAsync(UnityWebRequest request, int retryCount)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("Ошибка авторизации: сервер не отвечает");
      return;
    }

    string parsedMessage = ErrorResponse.ParseErrorMessage(request.downloadHandler.text);

    if (parsedMessage == "Токен истёк")
    {
      if (retryCount < 1) // Разрешаем только одну повторную попытку
      {
        Debug.Log("Токен истёк, обновляем...");

        await TokenRefreshManager.RefreshTokenAsync();

        if (!string.IsNullOrEmpty(SecureStorage.GetData(APIConstants.StorageKeys.AccessToken)))
        {
          await SendAllLogsAsync(retryCount + 1); // Повторяем попытку отправки
        }
        else
        {
          Debug.LogError("Ошибка: не удалось обновить токены");
        }
      }
    }
    else if (request.responseCode == 400)
    {
      Debug.LogError($"Ошибка некорректных данных: ({parsedMessage}).");
    }
    else if (request.responseCode == 401)
    {
      // Ошибки клиента (например, неверные данные или некорректный токен)
      Debug.LogError($"Ошибка клиента: {parsedMessage} (код {request.responseCode}). Пользователь будет выведен из системы.");
      AuthManager.ClearTokensAndLogout();
    }
    else
    {
      // Любые другие ошибки
      Debug.LogError($"Неизвестная ошибка: {parsedMessage} (код {request.responseCode})");
    }
  }


  ///// <summary>
  ///// Загружает все файлы логов из директории.
  ///// </summary>
  //private static async Task<(List<string> contents, List<string> paths)> LoadAllLocalLogsAsync()
  //{
  //  List<string> logsContent = new List<string>();
  //  List<string> logsPath = new List<string>();

  //  if (Directory.Exists(_logsDirectoryPath))
  //  {
  //    foreach (var file in Directory.GetFiles(_logsDirectoryPath, "*.json"))
  //    {
  //      string content = await File.ReadAllTextAsync(file);
  //      logsContent.Add(content);
  //      logsPath.Add(file);
  //    }
  //  }
  //  else
  //  {
  //    Debug.LogError("Папка логов не обнаружена");
  //    LogSessionManager.CreateLogsDirectory();
  //  }

  //  return (logsContent, logsPath);
  //}

  ///// <summary>
  ///// Удаляет все отправленные логи.
  ///// </summary>
  //private static void DeleteSentLogs(List<string> filePaths)
  //{
  //  foreach (var path in filePaths)
  //  {
  //    if (File.Exists(path))
  //      File.Delete(path);
  //  }
  //}
}
