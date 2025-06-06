using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
  [SerializeField] private Button _buttonRestart;
  [SerializeField] private Button _buttonResume;

  [SerializeField] private TMP_Text _achievementResult;
  [SerializeField] private TMP_Text _textResult;

  void Start()
  {
    string gameId = SelectedGameData.SelectedGameId;
    string sceneToLoad = GameDataManager.GetSceneName(gameId);

    _achievementResult.text = GameResultsManager.GetAchievement();
    _textResult.text = GameResultsManager.GetResults();

    _buttonRestart.onClick.AddListener(() => SceneManager.LoadScene(sceneToLoad));
    _buttonResume.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.MenuCategoryGame));
  }
}
