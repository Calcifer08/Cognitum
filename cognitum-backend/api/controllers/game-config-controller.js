import GameConfig from "../models/game-config.js";

export const updateGameConfig = async (req, res) => {
  try {
    const userId = req.user._id;
    const { GamesConfigData } = req.body;

    if (!GamesConfigData || typeof GamesConfigData !== "object") {
      return res.status(400).json({ message: "Некорректные данные" });
    }

    let gameConfig = await GameConfig.findOne({ userId });

    if (!gameConfig) {
      gameConfig = new GameConfig({ userId });
    }

    for (const [game, newData] of Object.entries(GamesConfigData)) {
      if (!gameConfig.gamesConfigData.has(game)) {
        gameConfig.gamesConfigData.set(game, {});
      }

      const existingGameData = gameConfig.gamesConfigData.get(game).toObject();

      gameConfig.gamesConfigData.set(game, {
        ...existingGameData,
        ...newData,
      });
    }

    await gameConfig.save();

    res.status(200).json({ message: "Конфиг обновлён" });
  } catch (error) {
    console.error("Ошибка при обновлении конфига:", error);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};
