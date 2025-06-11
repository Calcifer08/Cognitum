using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class PlayerData
{
  public string UserID;

  [JsonConverter(typeof(StringEnumConverter))]
  public AgeGroup Age;

  [JsonConverter(typeof(StringEnumConverter))]
  public Genders Gender;

  [JsonConverter(typeof(StringEnumConverter))]
  public EducationLevel Education;

  [JsonConverter(typeof(StringEnumConverter))]
  public SleepDuration Sleep;

  [JsonConverter(typeof(StringEnumConverter))]
  public DigitalUsageHours DigitalUsage;

  public PlayerData(string userId)
  {
    UserID = userId;
    Age = AgeGroup.Unknown;
    Gender = Genders.Unknown;
    Education = EducationLevel.Unknown;
    Sleep = SleepDuration.Unknown;
    DigitalUsage = DigitalUsageHours.Unknown;
  }

  public static string GetEnumMemberValue<T>(T value) where T : Enum
  {
    var enumType = typeof(T);
    var enumName = Enum.GetName(enumType, value);
    var member = enumType.GetField(enumName);
    var attributes = (EnumMemberAttribute[])member.GetCustomAttributes(typeof(EnumMemberAttribute), false);
    return attributes.Length > 0 ? attributes[0].Value : enumName;
  }

  public enum AgeGroup
  {
    [EnumMember(Value = "Не указано")]
    Unknown,

    [EnumMember(Value = "10-14")]
    Age_10_14,

    [EnumMember(Value = "14-18")]
    Age_14_18,

    [EnumMember(Value = "18-25")]
    Age_18_25,

    [EnumMember(Value = "26-35")]
    Age_26_35,

    [EnumMember(Value = "36-45")]
    Age_36_45,

    [EnumMember(Value = "46-55")]
    Age_46_55,

    [EnumMember(Value = "56-65")]
    Age_56_65,

    [EnumMember(Value = "66+")]
    Age_66_Plus
  }

  public enum Genders
  {
    [EnumMember(Value = "Не указано")]
    Unknown,

    [EnumMember(Value = "Мужчина")]
    Male,

    [EnumMember(Value = "Женщина")]
    Female
  }

  public enum EducationLevel
  {
    [EnumMember(Value = "Не указано")]
    Unknown,

    [EnumMember(Value = "Среднее")]
    Secondary,

    [EnumMember(Value = "Высшее")]
    Higher,

    [EnumMember(Value = "Аспирантура и выше")]
    PostGrad
  }

  public enum SleepDuration
  {
    [EnumMember(Value = "Не указано")]
    Unknown,

    [EnumMember(Value = "7+ часов")]
    Good,

    [EnumMember(Value = "5-6 часов")]
    Medium,

    [EnumMember(Value = "<5 часов")]
    Bad
  }

  public enum DigitalUsageHours
  {
    [EnumMember(Value = "Не указано")]
    Unknown,

    [EnumMember(Value = "<1 часа")]
    Less_1,

    [EnumMember(Value = "1-3 часа")]
    Hours_1_3,

    [EnumMember(Value = "4-6 часов")]
    Hours_4_6,

    [EnumMember(Value = "7+ часов")]
    More_7
  }
}
