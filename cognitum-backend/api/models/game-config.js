import mongoose from "mongoose";

const gameSchema = new mongoose.Schema(
  {
    CurrentLevel: { type: Number, default: 1 },
    MaxLevelReached: { type: Number, default: 1 },
    CountGame: { type: Number, default: 0 },
    MaxScore: { type: Number, default: 0 },
  },
  { _id: false }
);

const gameConfigSchema = new mongoose.Schema({
  userId: { type: mongoose.Schema.Types.ObjectId, required: true, unique: true },
  gamesConfigData: {
    type: Map,
    of: gameSchema,
    default: {},
  },
});

const GameConfig = mongoose.model("GameConfig", gameConfigSchema);

export default GameConfig;
