import express from "express";
import { register, login, logout, refreshToken } from "../controllers/authController.js";
import authMiddleware from "../middleware/auth-middleware.js";

const router = express.Router();

router.post("/register", register);
router.post("/login", login);
router.post("/logout", authMiddleware, logout);

router.post("/refresh-token", refreshToken);

export default router;
