using UnityEngine;

public static class AndroidToast
{
  private static AndroidJavaClass _unityPlayer;
  private static AndroidJavaObject _activity;
  private static AndroidJavaClass _toastClass;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Initialize()
  {
    if (Application.platform != RuntimePlatform.Android) return;

    _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    _activity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    _toastClass = new AndroidJavaClass("android.widget.Toast");
  }

  public static void Show(string message, bool isLong = false)
  {
    if (Application.platform != RuntimePlatform.Android)
    {
      Debug.Log($"Toast не поддерживаются на данной платформе: {message}");
      return;
    }

    int duration = isLong ?
        _toastClass.GetStatic<int>("LENGTH_LONG") :
        _toastClass.GetStatic<int>("LENGTH_SHORT");

    _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
    {
      _toastClass.CallStatic<AndroidJavaObject>(
          "makeText",
          _activity,
          message,
          duration
      ).Call("show");
    }));
  }
}
