import express from "express";
import authMiddleware from "../middleware/auth-middleware.js";
import { updateGameStatistics } from "../controllers/game-statistics-controller.js";

const router = express.Router();

router.post("/update", authMiddleware, updateGameStatistics);

export default router;
