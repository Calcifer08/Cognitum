using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public static class DataSyncManager
{
  public const string FileName = "PendingFiles.json";
  private static readonly string _pendingFilesPath = Path.Combine(Application.persistentDataPath, FileName);
  private static HashSet<string> _pendingFiles = new HashSet<string>();
  private static bool _isSending = false;
  private static int _timeWait = 30 * 1000; // для Task.Delay

  /// <summary>
  /// Инициализация: загружаем список неотправленных файлов и пытаемся отправить.
  /// </summary>
  public static async Task InitializeAsync()
  {
    await LoadPendingFilesAsync();
    await TrySendDataAsync();
  }

  /// <summary>
  /// Загружает список неотправленных файлов из файла в память.
  /// </summary>
  private static async Task LoadPendingFilesAsync()
  {
    if (File.Exists(_pendingFilesPath))
    {
      string json = await File.ReadAllTextAsync(_pendingFilesPath);
      _pendingFiles = JsonConvert.DeserializeObject<HashSet<string>>(json) ?? new HashSet<string>();
    }
  }

  /// <summary>
  /// Сохраняет список неотправленных файлов в файл.
  /// </summary>
  private static async Task SavePendingFilesAsync()
  {
    string json = JsonConvert.SerializeObject(_pendingFiles);
    await File.WriteAllTextAsync(_pendingFilesPath, json);
  }

  /// <summary>
  /// Добавляет файл в очередь и сразу пытается отправить.
  /// </summary>
  public static async Task AddFileToQueueAsync(string fileName)
  {
    if (_pendingFiles.Add(fileName)) // Добавляем, если файла ещё нет в списке
    {
      await SavePendingFilesAsync(); // Сохраняем обновлённый список
      Debug.Log($"Файл {fileName} добавлен в очередь на отправку.");
    }
    await Task.Delay(_timeWait);
    await TrySendDataAsync();
  }

  /// <summary>
  /// Проверяет интернет и отправляет данные.
  /// </summary>
  private static async Task TrySendDataAsync()
  {
    if (_isSending) return;

    _isSending = true;

    while (_pendingFiles.Count > 0)
    {
      if (Application.internetReachability == NetworkReachability.NotReachable)
      {
        Debug.Log("Нет интернета, попытаемся отправить данные позже...");
        await Task.Delay(_timeWait);
        continue;
      }

      Debug.LogWarning("Попытка синхронизации");
      _isSending = true;
      List<string> filesToSend = new List<string>(_pendingFiles); // Копируем, чтобы не изменять коллекцию во время перебора

      foreach (string file in filesToSend)
      {
        bool success = await SendDataByFileNameAsync(file);

        if (success)
        {
          _pendingFiles.Remove(file);
          await SavePendingFilesAsync(); // Обновляем файл после удаления
        }
        else
        {
          Debug.Log("Ошибка при отправке данных, будет произведена повторная попытка через 1 час.");
          await Task.Delay(TimeSpan.FromHours(1)); // если сервер недоступен, то незачем спамить
          break;  // Выход из текущего цикла, чтобы позже продолжить с новых данных
        }
      }
    }

    _isSending = false;
  }

  /// <summary>
  /// Отправляет данные по имени файла.
  /// </summary>
  private static async Task<bool> SendDataByFileNameAsync(string fileName)
  {
    switch (fileName)
    {
      case PlayerDataManager.FileName:
        return await PlayerDataManager.SendPlayerDataFromQueueAsync();
      case GameConfigManager.FileName:
        return await GameConfigManager.SendAllGameConfigAsync();
      case GameStatisticsManager.FileName:
        return await GameStatisticsManager.SendAllStatisticsAsync();
      default:
        Debug.LogError($"Неизвестный файл {fileName}");
        return false;
    }
  }
}
