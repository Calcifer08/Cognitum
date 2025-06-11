using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

public class StatisticsUI : MonoBehaviour
{
  [SerializeField] private BarChart _barChart;

  [SerializeField] private TMP_Dropdown _categoryDropdown;
  [SerializeField] private TMP_Dropdown _gameDropdown;
  [SerializeField] private TMP_Dropdown _periodDropdown;
  [SerializeField] private Button _showStatisticsButton;
  [SerializeField] private TMP_Text _text;

  [SerializeField] private GameObject _content;
  [SerializeField] private GameObject _loadText;
  [SerializeField] private GameObject _graphContainer;

  private StatisticsUIManager _manager;

  private List<DropdownOption<string>> _categoryOptions;
  private List<DropdownOption<string>> _gameOptions;
  private List<DropdownOption<string>> _periodOptions;

  private async void Start()
  {
    _content.SetActive(false);
    _loadText.SetActive(true);

    Coroutine loadingCoroutine = StartCoroutine(AnimateLoadingText());

    _manager = new StatisticsUIManager();
    await _manager.InitAsync();

    if (this == null) return;

    InitCategoryDropdown();
    InitPeriodDropdown();

    _categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
    _showStatisticsButton.onClick.AddListener(OnShowStatisticsClicked);

    if (loadingCoroutine != null)
      StopCoroutine(loadingCoroutine);

    _loadText.SetActive(false);
    _content.SetActive(true);

    _text.gameObject.SetActive(false);
    _graphContainer.SetActive(false);

    LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());
  }

  void InitCategoryDropdown()
  {
    _categoryOptions = _manager.GetCategoryOptions();
    _categoryDropdown.ClearOptions();
    _categoryDropdown.AddOptions(_categoryOptions.Select(opt => new TMP_Dropdown.OptionData(opt.DisplayName)).ToList());

    if (_categoryOptions.Count > 0)
    {
      OnCategoryChanged(0);
    }
  }

  void InitPeriodDropdown()
  {
    _periodOptions = _manager.GetPeriodOptions();
    _periodDropdown.ClearOptions();
    _periodDropdown.AddOptions(_periodOptions.Select(opt => new TMP_Dropdown.OptionData(opt.DisplayName)).ToList());
  }

  void OnCategoryChanged(int index)
  {
    string selectedCategoryId = _categoryOptions[index].Value;

    _gameOptions = _manager.GetGameOptions(selectedCategoryId);
    _gameDropdown.ClearOptions();
    _gameDropdown.AddOptions(_gameOptions.Select(opt => new TMP_Dropdown.OptionData(opt.DisplayName)).ToList());
  }

  void OnShowStatisticsClicked()
  {
    string selectedCategoryId = _categoryOptions[_categoryDropdown.value].Value;
    string selectedGameId = _gameOptions[_gameDropdown.value].Value;
    string selectedPeriodKey = _periodOptions[_periodDropdown.value].Value;

    var data = _manager.GetChartData(selectedCategoryId, selectedGameId, selectedPeriodKey);

    _text.gameObject.SetActive(true);
    if (data.Count < 1)
    {
      _graphContainer.SetActive(false);
      _text.text = "Данных за выбранный период нет.";
    }
    else
    {
      _text.text = "Среднее значение рассчитывается по всем сыгранным сессиям в пределах указанного периода.";
      _graphContainer.SetActive(true);
      LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());
      _barChart.DrawBarChart(data);
    }
  }

  private IEnumerator AnimateLoadingText()
  {
    TMP_Text loadingTMP = _loadText.GetComponent<TMP_Text>();
    string baseText = "Данные обновляются ";
    int dotCount = 0;

    while (true)
    {
      dotCount = (dotCount % 3) + 1;
      loadingTMP.text = baseText + new string('.', dotCount);
      yield return new WaitForSeconds(0.5f);
    }
  }
}
