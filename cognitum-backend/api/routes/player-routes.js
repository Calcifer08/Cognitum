import express from "express";
import { savePlayerData } from "../controllers/player-controller.js";
import authMiddleware from "../middleware/auth-middleware.js";

const router = express.Router();

router.post("/update", authMiddleware, savePlayerData);

export default router;
