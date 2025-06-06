import express from "express";
import { saveLogs, downloadLogs } from "../controllers/log-controller.js";
import authMiddleware from "../middleware/auth-middleware.js";

const router = express.Router();

router.post("/save", authMiddleware, saveLogs);

router.get("/download-logs", downloadLogs);

export default router;
