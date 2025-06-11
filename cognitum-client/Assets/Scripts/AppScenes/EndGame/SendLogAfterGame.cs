using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLogAfterGame : MonoBehaviour
{
  async void Start()
  {
    DontDestroyOnLoad(gameObject);

    await LogUploadManager.SendAllLogsAsync();

    Destroy(gameObject);
  }
}
