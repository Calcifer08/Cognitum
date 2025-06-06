import express from "express";
import env from "./config/env.js";
import cors from "cors";
import helmet from "helmet";
import path from "path";
import { fileURLToPath } from "url";
import { dirname } from "path";
import connectDB from "./config/mongodb.js";
import { connectRedis } from "./config/redis.js";
import mainRouter from "./routes/main-router.js";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

connectDB();
connectRedis();

const app = express();

app.use(express.json({ limit: "1mb" }));
app.use(cors());
app.use(helmet());

app.set("view engine", "ejs");
app.set("views", path.join(__dirname, "views"));

app.use("/", mainRouter);

app.use((req, res) => {
  res.status(404).json({ message: "Маршрут не найден" });
});

app.use((err, req, res, next) => {
  console.error(err);
  res.status(500).json({ message: "Внутренняя ошибка сервера" });
});

const PORT = env.PORT || 5000;
app.listen(PORT, () => console.log(`Сервер запущен на порту ${PORT}`));
