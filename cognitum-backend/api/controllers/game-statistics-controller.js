import GameStatistics from "../models/game-statistics.js";

export const updateGameStatistics = async (req, res) => {
  try {
    const userId = req.user._id;
    const { LastUpdate, GamesStatistics } = req.body;

    if (!GamesStatistics || typeof GamesStatistics !== "object") {
      return res.status(400).json({ message: "Некорректные данные статистики" });
    }

    let statistics = await GameStatistics.findOne({ userId });

    if (!statistics) {
      statistics = new GameStatistics({ userId });
    }

    if (LastUpdate) {
      statistics.LastUpdate = LastUpdate;
    }

    for (const [category, games] of Object.entries(GamesStatistics)) {
      if (!statistics.GamesStatistics[category]) {
        statistics.GamesStatistics[category] = {};
      }

      const currentCategory = statistics.GamesStatistics[category];

      for (const [gameName, gameData] of Object.entries(games)) {
        if (!currentCategory[gameName]) {
          currentCategory[gameName] = {
            LastUpdate: gameData.LastUpdate || "",
            Statistics: {
              DailyAverage: {},
              WeeklyAverage: {},
              MonthlyAverage: {},
            },
          };
        }

        const existingGame = currentCategory[gameName];

        if (gameData.LastUpdate) {
          existingGame.LastUpdate = gameData.LastUpdate;
        }

        for (const key of ["DailyAverage", "WeeklyAverage", "MonthlyAverage"]) {
          if (gameData.Statistics?.[key]) {
            const incomingMap = gameData.Statistics[key];
            const targetMap = existingGame.Statistics[key];

            for (const [date, avgData] of Object.entries(incomingMap)) {
              targetMap[date] = avgData;
            }
          }
        }

        currentCategory[gameName] = existingGame;
      }

      statistics.GamesStatistics[category] = currentCategory;
    }

    await statistics.save();

    res.status(200).json({ message: "Статистика успешно обновлена" });
  } catch (error) {
    console.error("Ошибка при обновлении статистики:", error);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};
