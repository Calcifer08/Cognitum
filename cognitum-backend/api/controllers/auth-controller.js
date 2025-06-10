import ms from "ms";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import User from "../models/user.js";
import GameConfig from "../models/game-config.js";
import { generateAccessToken, generateRefreshToken } from "../utils/generate-tokens.js";
import env from "../config/env.js";
import { redisClient } from "../config/redis.js";
import GameStatistics from "../models/game-statistics.js";

export const register = async (req, res) => {
  try {
    const { email, password } = req.body;
    const existingUser = await User.findOne({ email });
    if (existingUser) return res.status(400).json({ message: "Пользователь уже существует" });

    const hashedPassword = await bcrypt.hash(password, 10);
    const newUser = new User({
      email,
      password: hashedPassword,
      playerData: {},
    });
    await newUser.save();

    const accessToken = generateAccessToken(newUser._id);
    const refreshToken = generateRefreshToken(newUser._id);

    const hashedRefreshToken = await bcrypt.hash(refreshToken, 10);
    await redisClient.set(`refresh:${newUser._id}`, hashedRefreshToken, {
      EX: ms(env.REFRESH_EXPIRES_IN) / 1000,
    });

    const playerData = {
      userid: newUser.playerData?._id ?? newUser._id,
      age: newUser.playerData?.Age ?? "не указано",
      gender: newUser.playerData?.Genderender ?? "не указано",
      education: newUser.playerData?.Education ?? "не указано",
      sleep: newUser.playerData?.Sleep ?? "не указано",
      digitalUsage: newUser.playerData?.DigitalUsage ?? "не указано",
    };

    const gameConfig = await GameConfig.findOne({ userId: newUser._id })
      .select("gamesConfigData")
      .lean();

    const gameStatistics = await GameStatistics.findOne({ userId: newUser._id })
      .select("-_id -__v")
      .lean();

    res.json({
      accessToken,
      refreshToken,
      playerData,
      gameConfigData: gameConfig.gamesConfigData,
      gameStatistics: gameStatistics,
    });
  } catch (error) {
    console.error("Server error:", error);

    res.status(500).json({ message: "Ошибка сервера" });
  }
};

export const login = async (req, res) => {
  try {
    const { email, password } = req.body;
    const user = await User.findOne({ email });
    if (!user) return res.status(400).json({ message: "Неверные учетные данные" });

    const isMatch = await bcrypt.compare(password, user.password);
    if (!isMatch) return res.status(400).json({ message: "Неверные учетные данные" });

    const accessToken = generateAccessToken(user._id);
    const refreshToken = generateRefreshToken(user._id);
    const hashedRefreshToken = await bcrypt.hash(refreshToken, 10);

    await redisClient.set(`refresh:${user._id}`, hashedRefreshToken, {
      EX: ms(env.REFRESH_EXPIRES_IN) / 1000,
    });

    const playerData = {
      userid: user.playerData?._id ?? user._id,
      age: user.playerData?.Age ?? "не указано",
      gender: user.playerData?.Gender ?? "не указано",
      education: user.playerData?.Education ?? "не указано",
      sleep: user.playerData?.Sleep ?? "не указано",
      digitalUsage: user.playerData?.DigitalUsage ?? "не указано",
    };

    const gameConfig = await GameConfig.findOne({ userId: user._id })
      .select("gamesConfigData")
      .lean();

    const gameStatistics = await GameStatistics.findOne({ userId: user._id })
      .select("-_id -__v")
      .lean();

    res.json({
      accessToken,
      refreshToken,
      playerData,
      gameConfigData: gameConfig.gamesConfigData,
      gameStatistics: gameStatistics,
    });
  } catch (error) {
    res.status(500).json({ message: "Ошибка сервера" });
  }
};

export const refreshToken = async (req, res) => {
  const { refreshToken } = req.body;
  if (!refreshToken) return res.status(403).json({ message: "Токен обновления не предоставлен" });

  try {
    const decoded = jwt.verify(refreshToken, env.REFRESH_SECRET);
    const userId = decoded.userId;

    const hashedRefreshToken = await redisClient.get(`refresh:${userId}`);
    if (!hashedRefreshToken) {
      return res.status(403).json({ message: "Не авторизован" });
    }

    const isTokenValid = await bcrypt.compare(refreshToken, hashedRefreshToken);
    if (!isTokenValid) {
      return res.status(403).json({ message: "Неверный токен" });
    }

    const newAccessToken = generateAccessToken(userId);
    const newRefreshToken = generateRefreshToken(userId);
    const newHashedRefreshToken = await bcrypt.hash(newRefreshToken, 10);

    await redisClient.set(`refresh:${userId}`, newHashedRefreshToken, {
      EX: ms(env.REFRESH_EXPIRES_IN) / 1000,
    });

    res.json({ accessToken: newAccessToken, refreshToken: newRefreshToken });
  } catch (error) {
    if (error.name === "TokenExpiredError") {
      return res.status(403).json({ message: "Рефреш-токен истёк" });
    } else if (error.name === "JsonWebTokenError") {
      return res.status(403).json({ message: "Неверный рефреш-токен" });
    }

    return res.status(500).json({ message: "Ошибка сервера" });
  }
};

export const logout = async (req, res) => {
  try {
    const userId = req.user._id;

    await redisClient.del(`refresh:${userId}`);

    res.json({ message: "Выход выполнен" });
  } catch (error) {
    res.status(500).json({ message: "Ошибка выхода" });
  }
};
