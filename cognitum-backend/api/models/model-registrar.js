import mongoose from "mongoose";
import logSchema from "./log-schema.js";

export const registerGameModels = async () => {
  try {
    const collections = await mongoose.connection.db.listCollections().toArray();

    const gameCollections = collections.filter((collection) => collection.name.endsWith("logs"));

    gameCollections.forEach((collection) => {
      const GameName = collection.name.replace("logs", "");
      const modelName = GameName.toLowerCase() + "Log";
      const collectionName = GameName.toLowerCase() + "logs";

      if (!mongoose.models[modelName]) {
        mongoose.model(modelName, logSchema, collectionName);
      }
    });
  } catch (error) {
    console.error("Ошибка при регистрации моделей:", error);
  }
};
