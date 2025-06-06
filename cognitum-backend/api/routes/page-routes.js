import express from "express";
import { getGameSelection } from "../controllers/log-controller.js";

const router = express.Router();

router.get("/", (req, res) => {
  res.redirect("/select-game");
});

router.get("/select-game", getGameSelection);

export default router;
