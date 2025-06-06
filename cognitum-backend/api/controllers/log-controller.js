import mongoose from "mongoose";
import logSchema from "../models/log-schema.js";

export const saveLogs = async (req, res) => {
  try {
    const logsData = req.body;
    if (!Array.isArray(logsData) || logsData.length === 0) {
      return res.status(400).json({ message: "Не переданы логи" });
    }

    for (let logData of logsData) {
      const GameName = logData.Game?.GameName;

      if (!GameName) {
        console.warn("Не указано имя игры");
        continue;
      }

      const modelName = GameName.toLowerCase() + "Log";
      const collectionName = GameName.toLowerCase() + "logs";

      const GameLogModel =
        mongoose.models[modelName] || mongoose.model(modelName, logSchema, collectionName);

      const log = new GameLogModel(logData);

      try {
        await log.save();
      } catch (saveError) {
        console.error(`Ошибка при сохранении лога игры ${GameName}: ${saveError.message}`);
        continue;
      }
    }

    res.status(201).json({ message: "Логи успешно сохранены" });
  } catch (error) {
    console.error(error);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};

export const getGameSelection = async (req, res) => {
  try {
    const collections = await mongoose.connection.db.listCollections().toArray();
    const games = collections
      .filter((collection) => collection.name.endsWith("logs"))
      .map((collection) => collection.name.replace("logs", ""));

    res.render("download-game-logs", { games });
  } catch (error) {
    console.error("Ошибка при получении коллекций:", error);
    res.status(500).send("Ошибка сервера");
  }
};

export const downloadLogs = async (req, res) => {
  const { game } = req.query;

  if (!game) {
    return res.status(400).send("Не выбрана игра");
  }

  try {
    const GameLogModel = mongoose.model(game.toLowerCase() + "Log");
    const logs = await GameLogModel.find().lean();

    if (logs.length === 0) {
      return res.status(404).send("Логи для этой игры не найдены");
    }

    const cleanedLogs = logs.map(({ _id, __v, ...log }) => ({
      _id,
      ...JSON.parse(
        JSON.stringify(log, (key, value) => (key === "_id" || key === "__v" ? undefined : value))
      ),
    }));

    res.setHeader("Content-Type", "application/json");
    res.setHeader("Content-Disposition", `attachment; filename="${game}.json"`);
    res.send(JSON.stringify(cleanedLogs, null, 2));
  } catch (error) {
    console.error("Ошибка при выгрузке логов:", error);
    res.status(500).send("Ошибка при выгрузке логов");
  }
};
