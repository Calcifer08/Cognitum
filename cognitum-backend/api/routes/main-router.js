import express from "express";
import authRoutes from "./auth-routes.js";
import logsRoutes from "./logs-routes.js";
import playerRoutes from "./player-routes.js";
import gameConfigRoutes from "./game-config-routes.js";
import gameStatisticsRoutes from "./game-statistics-routes.js";
import passwordResetRoutes from "./password-reset-routes.js";
import pageRoutes from "./page-routes.js";

const router = express.Router();

router.use("/api/auth", authRoutes);
router.use("/api/logs", logsRoutes);
router.use("/api/player", playerRoutes);
router.use("/api/game-config", gameConfigRoutes);
router.use("/api/statistics", gameStatisticsRoutes);
router.use("/api/password-reset", passwordResetRoutes);

router.use("/", pageRoutes);

export default router;
