import GameStatistics from "../models/game-statistics.js";

export const updateGameStatistics = async (req, res) => {
  try {
    console.log("=== Получен запрос на обновление статистики ===");
    console.log("Пользователь:", req.user._id);
    console.log("Тело запроса:", JSON.stringify(req.body, null, 2));

    const userId = req.user._id;
    const { LastUpdate, GamesStatistics } = req.body;

    if (!GamesStatistics || typeof GamesStatistics !== "object") {
      console.warn("Некорректные данные статистики:", GamesStatistics);
      return res.status(400).json({ message: "Некорректные данные статистики" });
    }

    let statistics = await GameStatistics.findOne({ userId });

    if (!statistics) {
      console.log("Статистика для пользователя не найдена, создаём новую");
      statistics = new GameStatistics({ userId });
    } else {
      console.log("Найдена существующая статистика для пользователя");
    }

    if (LastUpdate) {
      console.log("Обновляем поле LastUpdate:", LastUpdate);
      statistics.LastUpdate = LastUpdate;
    }

    for (const [category, games] of Object.entries(GamesStatistics)) {
      if (!statistics.GamesStatistics[category]) {
        statistics.GamesStatistics[category] = {};
        console.log(`Добавляем новую категорию статистики: ${category}`);
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
          console.log(`Добавляем новую игру в категории ${category}: ${gameName}`);
        }

        const existingGame = currentCategory[gameName];

        if (gameData.LastUpdate) {
          console.log(`Обновляем LastUpdate игры ${gameName}:`, gameData.LastUpdate);
          existingGame.LastUpdate = gameData.LastUpdate;
        }

        for (const key of ["DailyAverage", "WeeklyAverage", "MonthlyAverage"]) {
          if (gameData.Statistics?.[key]) {
            const incomingMap = gameData.Statistics[key];
            const targetMap = existingGame.Statistics[key];

            for (const [date, avgData] of Object.entries(incomingMap)) {
              console.log(`Обновляем ${key} для игры ${gameName} на дату ${date}:`, avgData);
              targetMap[date] = avgData;
            }
          }
        }

        currentCategory[gameName] = existingGame;
      }

      statistics.GamesStatistics[category] = currentCategory;
    }

    statistics.markModified("GamesStatistics");
    await statistics.save();
    console.log("Статистика успешно сохранена в базе");

    res.status(200).json({ message: "Статистика успешно обновлена" });
  } catch (error) {
    console.error("Ошибка при обновлении статистики:", error);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};
