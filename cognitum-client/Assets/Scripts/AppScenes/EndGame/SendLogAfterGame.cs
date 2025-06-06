using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLogAfterGame : MonoBehaviour
{
  async void Start()
  {
    // иначе при смене сцены - прервётся отправка
    DontDestroyOnLoad(gameObject);

    await LogUploadManager.SendAllLogsAsync();

    Destroy(gameObject);
  }
}
