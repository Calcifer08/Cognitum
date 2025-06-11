using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CategoryMenu : MonoBehaviour
{
  [System.Serializable]
  public class CategoryVisualData
  {
    public string CategoryName;
    public Sprite BackgroundSprite;
    public Sprite IconSprite;
    public Color TextColor = Color.white;
  }

  [SerializeField] private Transform _boxGames;
  [SerializeField] private GameObject _prefabGame;
  [SerializeField] private GameObject _prefabTitle;
  [SerializeField] private TMP_Dropdown _categoryDropdown;

  private GamesData _gamesData;

  [SerializeField] private AllAboutGame _allAboutGame;

  [SerializeField] private List<CategoryVisualData> _categoryVisualList;

  private List<GameObject> _listObjects = new List<GameObject>();

  private void Start()
  {
    _gamesData = GameDataManager.GetGamesData();

    InitializationDropdown();
    DisplayCategory("Все");
    _categoryDropdown.onValueChanged.AddListener((_) => FilterCategory());
  }

  private void InitializationDropdown()
  {
    _categoryDropdown.options.Clear();

    _categoryDropdown.AddOptions(new List<string> { "Все" });

    foreach (var category in _gamesData.Categories)
    {
      _categoryDropdown.AddOptions(new List<string> { category.NameCategory });
    }
  }

  private CategoryVisualData GetVisualData(string categoryName)
  {
    return _categoryVisualList.FirstOrDefault(v => v.CategoryName == categoryName);
  }

  private void DisplayCategory(string nameCategory)
  {
    foreach (var item in _listObjects)
    {
      Destroy(item);
    }
    _listObjects.Clear();

    foreach (var category in _gamesData.Categories)
    {
      if (nameCategory == "Все" || category.NameCategory == nameCategory)
      {
        GameObject categoryTitle = Instantiate(_prefabTitle, _boxGames);
        categoryTitle.GetComponent<TextMeshProUGUI>().text = category.NameCategory;
        _listObjects.Add(categoryTitle);

        CategoryVisualData visualData = GetVisualData(category.NameCategory);

        foreach (var game in category.Games)
        {
          GameObject gameButton = Instantiate(_prefabGame, _boxGames);

          Transform iconTransform = gameButton.transform.Find("Icon");
          Transform titleTransform = gameButton.transform.Find("Title");
          Transform descriptionTransform = gameButton.transform.Find("Description");

          TextMeshProUGUI titleTMP = titleTransform?.GetComponent<TextMeshProUGUI>();
          TextMeshProUGUI descriptionTMP = descriptionTransform?.GetComponent<TextMeshProUGUI>();

          if (titleTMP != null)
            titleTMP.text = game.NameGame;

          if (descriptionTMP != null)
          {
            AboutGame aboutGame = _allAboutGame.AboutGames.Find(t => t.GameId == game.GameId);
            descriptionTMP.text = aboutGame != null ? aboutGame.Description : "Описание не найдено";
          }

          if (visualData != null)
          {
            gameButton.GetComponent<Image>().sprite = visualData.BackgroundSprite;

            if (iconTransform != null)
              iconTransform.GetComponent<Image>().sprite = visualData.IconSprite;

            if (titleTMP != null)
              titleTMP.color = visualData.TextColor;

            if (descriptionTMP != null)
              descriptionTMP.color = visualData.TextColor;
          }

          string gameIdCopy = game.GameId;
          gameButton.GetComponent<Button>().onClick.AddListener(() => OnClick(gameIdCopy));

          _listObjects.Add(gameButton);
        }
      }
    }
  }

  private void FilterCategory()
  {
    string selectedCategory = _categoryDropdown.options[_categoryDropdown.value].text;
    DisplayCategory(selectedCategory);
  }

  private void OnClick(string gameID)
  {
    SelectedGameData.SelectedGameId = gameID;

    SceneManager.LoadScene(SceneNames.AboutGame);
  }
}
