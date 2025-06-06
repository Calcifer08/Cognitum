import jwt from "jsonwebtoken";
import env from "../config/env.js";

export const generateAccessToken = (userId) => {
  return jwt.sign({ userId }, env.JWT_SECRET, { expiresIn: env.JWT_EXPIRES_IN });
};

export const generateRefreshToken = (userId) => {
  return jwt.sign({ userId }, env.REFRESH_SECRET, { expiresIn: env.REFRESH_EXPIRES_IN });
};
