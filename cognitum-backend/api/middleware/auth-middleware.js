import jwt from "jsonwebtoken";
import env from "../config/env.js";
import User from "../models/user.js";

const authMiddleware = async (req, res, next) => {
  const token = req.headers["authorization"]?.split(" ")[1];

  if (!token) {
    return res.status(403).json({ message: "Токен не предоставлен" });
  }

  try {
    const decoded = jwt.verify(token, env.JWT_SECRET);
    const user = await User.findById(decoded.userId);

    if (!user) {
      return res.status(401).json({ message: "Пользователь не найден" });
    }

    req.user = user;
    next();
  } catch (error) {
    if (error.name === "TokenExpiredError") {
      return res.status(401).json({ message: "Токен истёк" });
    } else if (error.name === "JsonWebTokenError") {
      return res.status(401).json({ message: "Неверный токен" });
    }

    return res.status(500).json({ message: "Ошибка сервера" });
  }
};

export default authMiddleware;
