using UnityEngine;

public class SoundManager : MonoBehaviour
{
  public static SoundManager Instance { get; private set; }

  [SerializeField] private AudioSource soundSource;


  [SerializeField] private AudioClip _correctAnswerClip;
  [SerializeField] private AudioClip _incorrectAnswerClip;


  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);

      if (soundSource == null)
      {
        soundSource = gameObject.AddComponent<AudioSource>();
        soundSource.playOnAwake = false;
      }
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public void PlaySoundAnswer(bool isCorrectAnswer)
  {
    var settings = AppSettingsManager.GetSettings();
    if (!settings.Sound.Enabled) return;

    soundSource.volume = settings.Sound.Volume;
    AudioClip clip = isCorrectAnswer ? _correctAnswerClip : _incorrectAnswerClip;
    soundSource.PlayOneShot(clip);
  }
}
