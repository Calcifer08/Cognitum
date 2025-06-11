import express from "express";
import { getGameSelection } from "../controllers/log-controller.js";
import { renderResetPasswordPage } from "../controllers/password-reset-controller.js";

const router = express.Router();

router.get("/", (req, res) => {
  res.redirect("/select-game");
});

router.get("/select-game", getGameSelection);

router.get("/reset-password", renderResetPasswordPage);

export default router;
