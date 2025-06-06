using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class BarChart : MonoBehaviour
{
  [SerializeField] private RectTransform _graphContainer;
  [SerializeField] private GameObject _barPrefab;
  [SerializeField] private GameObject _labelBarPrefab;
  [SerializeField] private GameObject _labelDateBarPrefab;

  [SerializeField] private RectTransform _lineContainer;
  [SerializeField] private GameObject _horizontalLinePrefab;
  [SerializeField] private GameObject _lineLabelPrefab;

  [SerializeField] private int _maxBar = 7;
  private readonly float _sizeMultiplier = 0.90f;
  private int _round = 4;
  private int _lineCount = 5;

  public void DrawBarChart(Dictionary<string, int> data)
  {
    ClearContainer(_graphContainer);
    ClearContainer(_lineContainer);

    if (data == null || data.Count == 0)
    {
      Debug.LogWarning("Данные для графика пусты.");
      return;
    }

    var sorted = new List<KeyValuePair<string, int>>(data);
    sorted.Sort((a, b) => DateTime.Parse(a.Key).CompareTo(DateTime.Parse(b.Key)));

    int actualCount = Mathf.Min(sorted.Count, _maxBar);
    var recentData = sorted.Skip(sorted.Count - actualCount).ToList();

    int maxScore = 1;
    foreach (var pair in recentData)
      maxScore = Mathf.Max(maxScore, pair.Value);

    GetRoundStep(maxScore);

    int roundedMax = Mathf.CeilToInt((float)maxScore / _round) * _round;
    if (roundedMax == maxScore)
      roundedMax += _round;

    float containerWidth = _graphContainer.rect.width;
    float containerHeight = _graphContainer.rect.height * _sizeMultiplier;

    float k = 0.3f;

    float barWidth = containerWidth / (_maxBar + k * (_maxBar - 1));
    float spaceWidth = k * barWidth;

    for (int i = 0; i < _maxBar; i++)
    {
      GameObject barObject = Instantiate(_barPrefab, _graphContainer);
      RectTransform barRect = barObject.GetComponent<RectTransform>();
      barRect.sizeDelta = new Vector2(barWidth, 0f);

      if (i < actualCount)
      {
        var pair = recentData[i];
        float normalizedHeight = (float)pair.Value / roundedMax;
        float barHeight = normalizedHeight * containerHeight;
        barRect.sizeDelta = new Vector2(barWidth, barHeight);

        // === Подпись даты ===
        GameObject labelObject = Instantiate(_labelDateBarPrefab, barObject.transform);
        TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();
        labelText.text = DateTime.Parse(pair.Key).ToString("dd.MM");

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0);
        labelRect.anchorMax = new Vector2(0.5f, 0);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.sizeDelta = new Vector2(barWidth, 50f);
        labelRect.anchoredPosition = new Vector2(0, 0);

        // === Подпись очков над столбцом ===
        GameObject valueLabel = Instantiate(_labelBarPrefab, barObject.transform);
        TextMeshProUGUI valueText = valueLabel.GetComponent<TextMeshProUGUI>();
        valueText.text = pair.Value.ToString();

        RectTransform valueRect = valueLabel.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.5f, 1f);
        valueRect.anchorMax = new Vector2(0.5f, 1f);
        valueRect.pivot = new Vector2(0.5f, 0);
        valueRect.sizeDelta = new Vector2(barWidth, 50f);
        valueRect.anchoredPosition = new Vector2(0, 10f);
      }
    }

    DrawGridLines(maxScore, roundedMax);
  }

  private void DrawGridLines(int maxScore, int roundedMax)
  {
    foreach (Transform child in _lineContainer)
      Destroy(child.gameObject);

    if (maxScore <= 0) return;

    float containerHeight = _lineContainer.rect.height * _sizeMultiplier;

    for (int i = 0; i < _lineCount; i++)
    {
      GameObject lineObject = Instantiate(_horizontalLinePrefab, _lineContainer);
      RectTransform lineRect = lineObject.GetComponent<RectTransform>();

      float normalized = (float)i / (_lineCount - 1);
      float yPos = normalized * containerHeight;

      lineRect.anchorMin = new Vector2(0, 0);
      lineRect.anchorMax = new Vector2(1, 0);
      lineRect.pivot = new Vector2(0.5f, 0.5f);
      lineRect.anchoredPosition = new Vector2(0, yPos);
      lineRect.sizeDelta = new Vector2(0, 2f);

      GameObject labelObject = Instantiate(_lineLabelPrefab, lineObject.transform);
      TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();

      int value = Mathf.RoundToInt(normalized * roundedMax);
      labelText.text = value.ToString();

      RectTransform labelRect = labelText.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0f, 0.5f);
      labelRect.anchorMax = new Vector2(0f, 0.5f);
      labelRect.pivot = new Vector2(1f, 0.5f);
      labelRect.anchoredPosition = new Vector2(-10f, 0f);
      labelRect.sizeDelta = new Vector2(70f, 50f);
    }
  }

  void ClearContainer(RectTransform container)
  {
    foreach (Transform child in container)
      Destroy(child.gameObject);
  }

  private void GetRoundStep(int maxScore)
  {
    _round = maxScore switch
    {
      int n when (n <= 20) => 4,
      int n when (n > 20 && n < 100) => 10,
      int n when (n >= 100 && n < 200) => 20,
      int n when (n >= 200 && n < 500) => 50,
      int n when (n >= 500) => 100,
      _ => 4,
    };
  }
}
