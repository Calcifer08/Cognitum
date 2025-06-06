import { createClient } from "redis";
import env from "./env.js";

const redisClient = createClient({
  socket: {
    host: env.REDIS_HOST || "localhost",
    port: env.REDIS_PORT || 6379,
  },
});

redisClient.on("error", (err) => console.error("Ошибка Redis:", err));

const connectRedis = async () => {
  if (!redisClient.isOpen) {
    try {
      await redisClient.connect();
      console.log("Redis подключен");
    } catch (error) {
      console.error("Ошибка подключения к Redis:", error);
      process.exit(1);
    }
  }
};

export { redisClient, connectRedis };
