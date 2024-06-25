using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace WeatherRegistry
{
  public class Rarity
  {
    private int _weight;
    public int Weight
    {
      get { return _weight; }
      set { _weight = Math.Clamp(value, 0, 10000); }
    }
  }

  public class NameRarity : Rarity
  {
    public string Name { get; set; }
  }

  public class LevelRarity : Rarity
  {
    public SelectableLevel Level { get; set; }
  }

  public class WeatherRarity : Rarity
  {
    public Weather Weather { get; set; }
  }

  public class ConfigHandler<T, CT>
  {
    // i need to make some stupid shit
    // basically a property that wraps around Bepinex ConfigEntry
    // so a getter would read that and resolve to my types
    // and a setter that does <something>

    // and this will resolve to string
    // so bepinex won't shit the bed hehe

    internal ConfigEntry<CT> ConfigEntry { get; }

    internal CT DefaultValue { get; }

    public virtual T Value { get; } = default(T);

    public ConfigHandler(CT defaultValue, Weather weather, string configTitle, ConfigDescription configDescription = null)
    {
      Plugin.logger.LogFatal($"Called Init constructor with default value: {defaultValue}");

      DefaultValue = defaultValue;
      string configCategory = $"Weather: {weather.name}{(weather.Origin != WeatherOrigin.Vanilla ? $" ({weather.Origin})" : "")}";

      ConfigEntry = ConfigManager.configFile.Bind(configCategory, configTitle, DefaultValue, configDescription);
    }
  }

  public class LevelListConfigHandler : ConfigHandler<SelectableLevel[], string>
  {
    public LevelListConfigHandler(string defaultValue, Weather weather, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, weather, configTitle, configDescription) { }

    public override SelectableLevel[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToLevels(ConfigEntry.Value);
      }
    }
  }

  public class LevelWeightsConfigHandler : ConfigHandler<LevelRarity[], string>
  {
    public LevelWeightsConfigHandler(string defaultValue, Weather weather, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, weather, configTitle, configDescription) { }

    public override LevelRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToLevelRarities(ConfigEntry.Value);
      }
    }
  }

  public class WeatherWeightsConfigHandler : ConfigHandler<WeatherRarity[], string>
  {
    public WeatherWeightsConfigHandler(string defaultValue, Weather weather, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, weather, configTitle, configDescription) { }

    public override WeatherRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToWeatherWeights(ConfigEntry.Value);
      }
    }
  }

  public class IntegerConfigHandler : ConfigHandler<int, int>
  {
    public IntegerConfigHandler(int defaultValue, Weather weather, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, weather, configTitle, configDescription) { }

    public override int Value
    {
      get { return ConfigEntry.Value; }
    }
  }

  public class FloatConfigHandler : ConfigHandler<float, float>
  {
    public FloatConfigHandler(float defaultValue, Weather weather, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, weather, configTitle, configDescription) { }

    public override float Value
    {
      get { return ConfigEntry.Value; }
    }
  }

  internal class ConfigHelper
  {
    internal static Dictionary<string, SelectableLevel> _levelsDictionary = null;
    public static Dictionary<string, SelectableLevel> StringToLevel
    {
      get
      {
        if (_levelsDictionary != null)
        {
          return _levelsDictionary;
        }

        Dictionary<string, SelectableLevel> Levels = [];

        StartOfRound
          .Instance.levels.ToList()
          .ForEach(level =>
          {
            Levels.TryAdd(GetNumberlessName(level).ToLower(), level);
            Levels.TryAdd(level.PlanetName.ToLower(), level);
            Levels.TryAdd(level.name.ToLower(), level);
          });

        _levelsDictionary = Levels;

        // return the result of this setter
        return Levels;
      }
    }

    internal static Dictionary<string, Weather> _weathersDictionary = null;
    public static Dictionary<string, Weather> StringToWeather
    {
      get
      {
        if (_weathersDictionary != null)
        {
          return _weathersDictionary;
        }

        Dictionary<string, Weather> Weathers = [];

        WeatherManager
          .Weathers.ToList()
          .ForEach(weather =>
          {
            Weathers.TryAdd(weather.name.ToLower(), weather);
            Weathers.TryAdd(weather.Name.ToLower(), weather);
            Weathers.TryAdd(((int)weather.VanillaWeatherType).ToString(), weather);
          });

        _weathersDictionary = Weathers;

        // return the result of this setter
        return Weathers;
      }
    }

    public static SelectableLevel ResolveStringToLevel(string str)
    {
      return StringToLevel.GetValueOrDefault(str.ToLower());
    }

    public static Weather ResolveStringToWeather(string str)
    {
      return StringToWeather.GetValueOrDefault(str.ToLower());
    }

    // straight-up copied from LLL (it's so fucking useful)
    public static string GetNumberlessName(SelectableLevel level)
    {
      return new string(level.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
    }

    // convert string to array of strings
    public static string[] ConvertStringToArray(string str)
    {
      string[] output = str.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray().Select(s => s.Trim()).ToArray();

      return output;
    }

    public static SelectableLevel[] ConvertStringToLevels(string str)
    {
      string[] levelNames = ConvertStringToArray(str);
      Dictionary<string, SelectableLevel> Levels = StringToLevel;
      // StartOfRound.Instance.levels.ToDictionary(level => GetNumberlessName(level), level => level);
      List<SelectableLevel> output = [];

      if (levelNames.Count() == 0)
      {
        return [];
      }

      foreach (string level in levelNames)
      {
        SelectableLevel selectableLevel = ResolveStringToLevel(level);

        Plugin.logger.LogDebug($"String {level} resolved to selectable level: {selectableLevel} (is null: {selectableLevel == null})");

        if (output.Contains(selectableLevel))
        {
          continue;
        }

        output.Add(selectableLevel);
      }

      return output.ToArray();
    }

    public static NameRarity[] ConvertStringToRarities(string str)
    {
      string[] rarities = ConvertStringToArray(str);
      List<NameRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split(':');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
          continue;
        }

        output.Add(new NameRarity { Name = rarityData[0], Weight = weight });
      }

      return output.ToArray();
    }

    public static LevelRarity[] ConvertStringToLevelRarities(string str)
    {
      // i want to use the following format:
      // LevelName@Weight;LevelName@Weight;LevelName@Weight

      string[] rarities = ConvertStringToArray(str);
      List<LevelRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split('@');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
          continue;
        }

        SelectableLevel level = ResolveStringToLevel(rarityData[0]);

        if (level == null)
        {
          Plugin.logger.LogWarning($"Invalid level name: {rarityData[0]}");
          continue;
        }

        output.Add(new LevelRarity { Level = level, Weight = weight });
      }

      return output.ToArray();
    }

    public static WeatherRarity[] ConvertStringToWeatherWeights(string str)
    {
      // i want to use the following format:
      // Weather@Weight;Weather@Weight;Weather@Weight

      string[] rarities = ConvertStringToArray(str);
      List<WeatherRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split('@');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
          continue;
        }

        Weather weather = ResolveStringToWeather(rarityData[0]);

        if (weather == null)
        {
          Plugin.logger.LogWarning($"Invalid weather name: {rarityData[0]}");
          continue;
        }

        output.Add(new WeatherRarity { Weather = weather, Weight = weight });
      }

      return output.ToArray();
    }
  }
}
