using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StatisticsUIManager
{
  private GamesData _gamesData;
  private List<DropdownOption<string>> _periodOptions;

  public async Task InitAsync()
  {
    _gamesData = GameDataManager.GetGamesData();

    await GameStatisticsManager.AggregateDateAsync();

    _periodOptions = new List<DropdownOption<string>>
    {
      new DropdownOption<string>("Дни", "DailyAverage"),
      new DropdownOption<string>("Недели", "WeeklyAverage"),
      new DropdownOption<string>("Месяцы", "MonthlyAverage"),
    };
  }

  public List<DropdownOption<string>> GetCategoryOptions()
  {
    return _gamesData.Categories
      .Select(c => new DropdownOption<string>(c.NameCategory, c.CategoryId))
      .ToList();
  }

  public List<DropdownOption<string>> GetGameOptions(string categoryId)
  {
    var category = _gamesData.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
    return category?.Games
      .Select(g => new DropdownOption<string>(g.NameGame, g.GameId))
      .ToList() ?? new List<DropdownOption<string>>();
  }

  public List<DropdownOption<string>> GetPeriodOptions()
  {
    return _periodOptions;
  }

  public Dictionary<string, int> GetChartData(string categoryId, string gameId, string periodKey)
  {
    Debug.Log($"Ищем данные для графика: категория = {categoryId}, игра = {gameId}, период = {periodKey}");
    return GameStatisticsManager.GetPeriodData(categoryId, gameId, periodKey);
  }
}

