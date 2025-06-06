import mongoose from "mongoose";
import env from "./env.js";
import { registerGameModels } from "../models/model-registrar.js";

const connectDB = async () => {
  try {
    await mongoose.connect(env.MONGO_URI);
    console.log("MongoDB подключен");

    await registerGameModels();
  } catch (error) {
    console.error("Ошибка подключения к MongoDB:", error);
    process.exit(1);
  }
};

export default connectDB;
