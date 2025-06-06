import mongoose from "mongoose";
import {
  AgeGroups,
  Genders,
  EducationLevels,
  SleepDurations,
  DigitalUsages,
} from "./enums-profile.js";

const logSchema = new mongoose.Schema({
  Player: {
    UserID: { type: String, required: true },
    Age: { type: String, enum: AgeGroups, required: true },
    Gender: { type: String, enum: Genders, required: true },
    Education: { type: String, enum: EducationLevels, required: true },
    Sleep: { type: String, enum: SleepDurations, required: true },
    DigitalUsage: { type: String, enum: DigitalUsages, required: true },
  },
  Device: {
    DeviceScreenWidth: { type: Number, required: true },
    DeviceScreenHeight: { type: Number, required: true },
    DeviceOS: { type: String, required: true },
  },
  Game: {
    GameName: { type: String, required: true },
    GameVersion: { type: String, required: true },
  },
  Session: {
    SessionParams: {
      AllowedTime: { type: Number, required: true },
      BaseSeed: { type: Number, required: true },
    },
    SessionResults: {
      TimeZone: { type: String, required: true },
      TimeStart: { type: Date, required: true },
      TimeEnd: { type: Date, required: true },
      StartLevel: { type: Number, required: true },
      FinalLevel: { type: Number, required: true },
      MaxLevelReached: { type: Number, required: true },
      MaxLevelCompleted: { type: Number, required: true },
      LevelsCompleted: { type: Number, required: true },
      CorrectQuestions: { type: Number, required: true },
      MistakeQuestions: { type: Number, required: true },
      CorrectAnswers: { type: Number, required: true },
      MistakeAnswers: { type: Number, required: true },
      SkippedAnswers: { type: Number, required: true },
    },
  },
  Levels: [
    {
      LevelParams: {
        Level: { type: Number, required: true },
        StreakCorrectQuestionsToLevelUp: { type: Number, required: true },
        CountMistakesToLevelDown: { type: Number, required: true },
        CountAnswersToQuestion: { type: Number, required: true },
        CountMistakesForQuestion: { type: Number, required: true },
        TimeMemorizePhase: { type: Number, required: true },
        TimeForgetPhase: { type: Number, required: true },
        TimeAnswerPhase: { type: Number, required: true },
        ResultDisplayTime: { type: Number, required: true },
        SpecificData: { type: mongoose.Schema.Types.Mixed, required: true },
      },
      LevelResults: {
        TimeStart: { type: String, required: true },
        TimeEnd: { type: String, required: true },
        CorrectQuestions: { type: Number, required: true },
        MistakeQuestions: { type: Number, required: true },
        CorrectAnswers: { type: Number, required: true },
        MistakeAnswers: { type: Number, required: true },
        SkippedAnswers: { type: Number, required: true },
        CompletionTime: { type: Number, required: true },
        ReactionTime: { type: Number, required: true },
        Status: { type: String, required: true },
      },
      LevelEvents: [
        {
          Time: { type: String, required: true },
          Tags: { type: String, required: true },
          EventData: { type: mongoose.Schema.Types.Mixed, required: true },
        },
      ],
    },
  ],
});

export default logSchema;
