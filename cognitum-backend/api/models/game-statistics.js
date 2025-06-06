import mongoose from "mongoose";

export function getCurrentDateTimeString() {
  const now = new Date();
  return now.toISOString().replace("T", " ").slice(0, 16);
}

const averageSchema = new mongoose.Schema(
  {
    AverageScore: { type: Number, default: 0 },
  },
  { _id: false }
);

const gameStatsDataSchema = new mongoose.Schema(
  {
    LastUpdate: { type: String, default: null },
    Statistics: {
      DailyAverage: {
        type: Object,
        default: {},
      },
      WeeklyAverage: {
        type: Object,
        default: {},
      },
      MonthlyAverage: {
        type: Object,
        default: {},
      },
    },
  },
  { _id: false }
);

const categorySchema = new mongoose.Schema(
  {
    type: Object,
    default: {},
  },
  { _id: false }
);

const gameStatisticsSchema = new mongoose.Schema({
  userId: { type: mongoose.Schema.Types.ObjectId, required: true, unique: true },
  LastUpdate: { type: String, default: () => getCurrentDateTimeString() },
  GamesStatistics: {
    type: Object,
    default: {},
  },
});

const GameStatistics = mongoose.model("GameStatistics", gameStatisticsSchema);

export default GameStatistics;
