import dotenv from "dotenv";

dotenv.config();

export default {
  PORT: process.env.PORT || 5000,

  MONGO_URI: process.env.MONGO_URI,

  REDIS_HOST: process.env.REDIS_HOST || "localhost",
  REDIS_PORT: process.env.REDIS_PORT || 6379,

  JWT_SECRET: process.env.JWT_SECRET,
  REFRESH_SECRET: process.env.REFRESH_SECRET,
  JWT_EXPIRES_IN: "15m",
  REFRESH_EXPIRES_IN: "30d",
};
