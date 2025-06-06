import User from "../models/user.js";

export const savePlayerData = async (req, res) => {
  try {
    const userId = req.user._id;
    const { Age, Gender, Education, Sleep, DigitalUsage } = req.body;

    const updatedUser = await User.findByIdAndUpdate(
      userId,
      {
        $set: {
          "playerData.Age": Age || "Не указано",
          "playerData.Gender": Gender || "Не указано",
          "playerData.Education": Education || "Не указано",
          "playerData.Sleep": Sleep || "Не указано",
          "playerData.DigitalUsage": DigitalUsage || "Не указано",
        },
      },
      { new: true, runValidators: true }
    );

    if (!updatedUser) {
      return res.status(404).json({ message: "Пользователь не найден" });
    }

    res.json({ message: "Данные успешно сохранены" });
  } catch (error) {
    console.error("Ошибка сохранения данных:", error);
    res.status(500).json({ message: "Ошибка сервера" });
  }
};
