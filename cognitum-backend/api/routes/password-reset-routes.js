import express from "express";
import { requestPasswordReset, resetPassword } from "../controllers/password-reset-controller.js";

const router = express.Router();

router.post("/request", requestPasswordReset);
router.post("/reset", resetPassword);

export default router;
