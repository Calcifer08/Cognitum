
[System.Serializable]
public class AppSettings
{
  public SoundSettings Sound = new SoundSettings();
  public NotificationSettings Notifications = new NotificationSettings();
  // в будущем можно добавить другие блоки
}

[System.Serializable]
public class SoundSettings
{
  public bool Enabled = true;
  public float Volume = 0.5f;
}

[System.Serializable]
public class NotificationSettings
{
  public bool Enabled = true;
  public int Hour = 18;
  public int Minute = 0;
}
