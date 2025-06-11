import bcrypt from "bcryptjs";
import crypto from "crypto";
import env from "../config/env.js";
import User from "../models/user.js";
import { redisClient } from "../config/redis.js";
import { sendPasswordResetEmail } from "../utils/send-email.js";

export const renderResetPasswordPage = async (req, res) => {
  try {
    const { token, id } = req.query;

    if (!token || !id) {
      return res.status(400).send("Неверная ссылка для сброса пароля.");
    }

    const tokenKey = `reset:${id}`;
    const hashedToken = await redisClient.get(tokenKey);

    if (!hashedToken) {
      return res.render("reset-password", {
        token: null,
        id: null,
        message: "Ссылка истекла или недействительна.",
      });
    }

    res.render("reset-password", { token, id, message: null });
  } catch (err) {
    console.error("Ошибка при рендере страницы сброса пароля:", err);
    res.status(500).send("Ошибка сервера");
  }
};

export const requestPasswordReset = async (req, res) => {
  try {
    const { email } = req.body;
    const user = await User.findOne({ email });

    if (user) {
      const token = crypto.randomBytes(32).toString("hex");
      const tokenKey = `reset:${user._id}`;
      const hashedToken = await bcrypt.hash(token, 10);
      await redisClient.set(tokenKey, hashedToken, { EX: 60 * 30 });

      const resetUrl = `${env.FRONTEND_URL}/reset-password?token=${token}&id=${user._id}`;
      await sendPasswordResetEmail(email, resetUrl);
    }

    res.status(200).json({ message: "Ссылка для сброса пароля отправлена на email." });
  } catch (err) {
    console.error("Ошибка при отправке запроса на сброс:", err);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};

export const resetPassword = async (req, res) => {
  try {
    const { id, token, newPassword } = req.body;

    if (!id || !token || !newPassword) {
      return res.status(400).render("reset-password", {
        token,
        id,
        message: "Неверные данные запроса",
      });
    }

    const tokenKey = `reset:${id}`;
    const hashedToken = await redisClient.get(tokenKey);

    if (!hashedToken) {
      return res.status(400).render("reset-password", {
        token: null,
        id: null,
        message: "Токен не найден или истёк",
      });
    }

    const isValid = await bcrypt.compare(token, hashedToken);

    if (!isValid) {
      return res.status(403).render("reset-password", {
        token: null,
        id: null,
        message: "Неверный токен",
      });
    }

    const hashedPassword = await bcrypt.hash(newPassword, 10);
    await User.findByIdAndUpdate(id, { password: hashedPassword });
    await redisClient.del(tokenKey);

    res.render("reset-password", {
      token: null,
      id: null,
      message: "Пароль успешно обновлён.",
    });
  } catch (err) {
    console.error("Ошибка сброса пароля:", err);
    res.status(500).render("reset-password", {
      token: null,
      id: null,
      message: "Ошибка сервера",
    });
  }
};
