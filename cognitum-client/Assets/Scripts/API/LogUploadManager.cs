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
/// ����� ��� �������� ���� ���������� ����� �� ������.
/// </summary>
public static class LogUploadManager
{
  private static readonly string _logsDirectoryPath = Path.Combine(Application.persistentDataPath, LogSessionManager.Folder);
  private static readonly int _batchSize = 5; // ������ �����
  private static readonly float _delayBetweenBatches = 1f; //  �������� ����� ���������� ������

  /// <summary>
  /// �������� ��� �������� ���� ����������� �����.
  /// </summary>
  /// 
  public static async Task SendAllLogsAsync(int retryCount = 0)
  {
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      Debug.Log("��� ���������, ���� ����� ���������� �����");
      return;
    }

    if (!Directory.Exists(_logsDirectoryPath))
    {
      Debug.LogWarning("���������� � ������ �� ����������. ���� �� ����� ����������.");
      return; // ����� �� �������, ���� ���������� �� ����������
    }

    var logFiles = Directory.GetFiles(_logsDirectoryPath, "*.json");

    // ���� ����� ���, �� ������� ��������� � �������
    if (logFiles.Length == 0)
    {
      Debug.Log("��� ����� ��� ��������");
      return;
    }

    string token = AuthManager.GetAccessToken();
    if (string.IsNullOrEmpty(token))
    {
      Debug.Log("������: ��� ������ ��� �������� �����");
      AuthManager.ClearTokensAndLogout();
      return;
    }

    for (int i = 0; i < logFiles.Length; i += _batchSize)
    {
      // ������ ���� �����
      var batchFiles = logFiles.Skip(i).Take(_batchSize).ToList();
      var batchContents = new List<string>();

      foreach (var file in batchFiles)
      {
        string content = await File.ReadAllTextAsync(file);
        batchContents.Add(content);
      }

      // ���������� JSON-������ � ������
      string batchJson = $"[{string.Join(",", batchContents)}]";

      using (UnityWebRequest request = new UnityWebRequest(APIConstants.ApiEndpoints.SaveLogsUrl, "POST"))
      {
        byte[] postData = Encoding.UTF8.GetBytes(batchJson);
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.SetRequestHeader("Content-Type", "application/json");

        // ������� ���������� ������� ����������
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
          await Task.Yield();  // ����������� ����� ��� ������ �����
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
#if !UNITY_EDITOR
          // ������� ����� ����� ����� �������� �����
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

          // ���������� ����� � ����� DeleteLogs
          foreach (var file in batchFiles)
          {
            string fileName = Path.GetFileName(file);
            string newFilePath = Path.Combine(deleteLogsFolder, fileName);

            try
            {
              File.Move(file, newFilePath); // ���������� ����
              Debug.Log($"���� {fileName} ��������� � ����� DeleteLogs.");
            }
            catch (Exception ex)
            {
              Debug.LogError($"�� ������� ����������� ���� {fileName}: {ex.Message}");
            }
          }
#endif
          Debug.Log($"���������� {i + batchContents.Count} �� {logFiles.Length} �����");
        }
        else
        {
          await HandleLogErrorResponseAsync(request, retryCount);
          return;
        }
      }

      // ��������� �������� ����� �������
      await Task.Delay((int)(_delayBetweenBatches * 1000)); // ����������� �������� � ������������
    }

    Debug.Log("��� ���� ������� ���������� � �������");
  }

  /// <summary>
  /// ������������ ������, ���������� ��� �������� �����.
  /// </summary>
  private static async Task HandleLogErrorResponseAsync(UnityWebRequest request, int retryCount)
  {
    if (request.responseCode == 0)
    {
      Debug.LogError("������ �����������: ������ �� ��������");
      return;
    }

    string parsedMessage = ErrorResponse.ParseErrorMessage(request.downloadHandler.text);

    if (parsedMessage == "����� ����")
    {
      if (retryCount < 1) // ��������� ������ ���� ��������� �������
      {
        Debug.Log("����� ����, ���������...");

        await TokenRefreshManager.RefreshTokenAsync();

        if (!string.IsNullOrEmpty(SecureStorage.GetData(APIConstants.StorageKeys.AccessToken)))
        {
          await SendAllLogsAsync(retryCount + 1); // ��������� ������� ��������
        }
        else
        {
          Debug.LogError("������: �� ������� �������� ������");
        }
      }
    }
    else if (request.responseCode == 400)
    {
      Debug.LogError($"������ ������������ ������: ({parsedMessage}).");
    }
    else if (request.responseCode == 401)
    {
      // ������ ������� (��������, �������� ������ ��� ������������ �����)
      Debug.LogError($"������ �������: {parsedMessage} (��� {request.responseCode}). ������������ ����� ������� �� �������.");
      AuthManager.ClearTokensAndLogout();
    }
    else
    {
      // ����� ������ ������
      Debug.LogError($"����������� ������: {parsedMessage} (��� {request.responseCode})");
    }
  }


  ///// <summary>
  ///// ��������� ��� ����� ����� �� ����������.
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
  //    Debug.LogError("����� ����� �� ����������");
  //    LogSessionManager.CreateLogsDirectory();
  //  }

  //  return (logsContent, logsPath);
  //}

  ///// <summary>
  ///// ������� ��� ������������ ����.
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
