using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialMenu : MonoBehaviour
{
  void Start()
  {
    SceneManager.LoadScene(SceneNames.BurgerMenu, LoadSceneMode.Additive);
  }
}
