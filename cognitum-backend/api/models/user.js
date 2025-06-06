import mongoose from "mongoose";
import {
  AgeGroups,
  Genders,
  EducationLevels,
  SleepDurations,
  DigitalUsages,
} from "./enums-profile.js";
import GameConfig from "./game-config.js";
import GameStatistics from "./game-statistics.js";
import { getCurrentDateTimeString } from "./game-statistics.js";

const playerDataSchema = new mongoose.Schema(
  {
    Age: { type: String, default: "Не указано", enum: AgeGroups },
    Gender: { type: String, default: "Не указано", enum: Genders },
    Education: { type: String, default: "Не указано", enum: EducationLevels },
    Sleep: { type: String, default: "Не указано", enum: SleepDurations },
    DigitalUsage: { type: String, default: "Не указано", enum: DigitalUsages },
  },
  { _id: false }
);

const userSchema = new mongoose.Schema(
  {
    email: { type: String, required: true, unique: true },
    password: { type: String, required: true },
    playerData: { type: playerDataSchema, default: {} },
    gameConfig: { type: mongoose.Schema.Types.ObjectId, ref: "GameConfig" },
    gameStatistics: { type: mongoose.Schema.Types.ObjectId, ref: "GameStatistics" },
  },
  { timestamps: true }
);

userSchema.pre("save", async function (next) {
  if (this.isNew) {
    const defaultGameConfig = new GameConfig({
      userId: this._id,
      gamesConfigData: {},
    });

    await defaultGameConfig.save();

    this.gameConfig = defaultGameConfig._id;

    const defaultGameStatistics = new GameStatistics({
      userId: this._id,
      LastUpdate: getCurrentDateTimeString(),
      GamesStatistics: {},
    });

    await defaultGameStatistics.save();
    this.gameStatistics = defaultGameStatistics._id;
  }

  next();
});

export default mongoose.model("User", userSchema);
