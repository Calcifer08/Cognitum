import express from "express";
import authMiddleware from "../middleware/auth-middleware.js";
import { updateGameConfig } from "../controllers/game-config-controller.js";

const router = express.Router();

router.post("/update", authMiddleware, updateGameConfig);

export default router;
