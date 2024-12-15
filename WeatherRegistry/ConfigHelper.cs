using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

  public abstract class ConfigHandler<T, CT> : Definitions.ConfigHandler<T, CT>
  {
    public ConfigFile ConfigFile { get; set; } = ConfigManager.configFile;

    public ConfigHandler(CT value, bool enabled = true)
    {
      DefaultValue = value;
      Enabled = enabled;
    }

    public void SetConfigEntry(Weather weather, string configTitle, ConfigDescription configDescription = null)
    {
      if (Enabled)
      {
        ConfigEntry = ConfigFile.Bind(weather.ConfigCategory, configTitle, DefaultValue, configDescription);
      }
      else
      {
        ConfigEntry = null;
        Plugin.debugLogger.LogDebug($"Config entry for {weather.Name}: {configTitle} is disabled");
      }
    }

    public void SetConfigEntry(string configCategory, string configTitle, ConfigDescription configDescription = null)
    {
      if (Enabled)
      {
        ConfigEntry = ConfigFile.Bind(configCategory, configTitle, DefaultValue, configDescription);
      }
      else
      {
        ConfigEntry = null;
        Plugin.debugLogger.LogDebug($"Config entry {configTitle} is disabled");
      }
    }
  }

  public class LevelListConfigHandler : ConfigHandler<SelectableLevel[], string>
  {
    public LevelListConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public LevelListConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override SelectableLevel[] Value
    {
      get { return ConfigHelper.ConvertStringToLevels(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue); }
    }
  }

  public class LevelWeightsConfigHandler : ConfigHandler<LevelRarity[], string>
  {
    public LevelWeightsConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public LevelWeightsConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override LevelRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToLevelRarities(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue);
      }
    }
  }

  public class WeatherWeightsConfigHandler : ConfigHandler<WeatherRarity[], string>
  {
    public WeatherWeightsConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public WeatherWeightsConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override WeatherRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToWeatherWeights(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue);
      }
    }
  }

  public class BooleanConfigHandler : ConfigHandler<bool, bool>
  {
    public BooleanConfigHandler(bool value, bool enabled = true)
      : base(value, enabled) { }

    public override bool Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class IntegerConfigHandler : ConfigHandler<int, int>
  {
    public IntegerConfigHandler(int value, bool enabled = true)
      : base(value, enabled) { }

    public override int Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class FloatConfigHandler : ConfigHandler<float, float>
  {
    public FloatConfigHandler(float value, bool enabled = true)
      : base(value, enabled) { }

    public override float Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class StringConfigHandler : ConfigHandler<string, string>
  {
    public StringConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public override string Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  internal class ConfigHelper
  {
    private static MrovLib.Logger logger = new("WeatherRegistry", ConfigManager.LogWeightResolving);

    private static Dictionary<string, Weather> _weathersDictionary = null;
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
            Weathers.TryAdd(weather.name.ToLowerInvariant(), weather);
            Weathers.TryAdd(weather.Name.ToLowerInvariant(), weather);
            Weathers.TryAdd(GetAlphanumericName(weather).ToLowerInvariant(), weather);
          });

        _weathersDictionary = Weathers;

        return Weathers;
      }
      set { _weathersDictionary = value; }
    }

    public static Weather ResolveStringToWeather(string str)
    {
      return StringToWeather.GetValueOrDefault(str.ToLowerInvariant());
    }

    // straight-up copied from LLL (it's so fucking useful)
    public static string GetNumberlessName(SelectableLevel level)
    {
      return MrovLib.StringResolver.GetNumberlessName(level);
    }

    public static string GetAlphanumericName(SelectableLevel level)
    {
      Regex regex = new(@"^[0-9]+|[-_/\\\ ]");
      return new string(regex.Replace(level.PlanetName, ""));
    }

    public static string GetAlphanumericName(Weather weather)
    {
      Regex regex = new(@"^[0-9]+|[-_/\\\ ]");
      return new string(regex.Replace(weather.Name, ""));
    }

    // convert string to array of strings
    public static string[] ConvertStringToArray(string str)
    {
      string[] output = str.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();

      return output;
    }

    public static SelectableLevel[] ConvertStringToLevels(string str)
    {
      return MrovLib.StringResolver.ResolveStringToLevels(str);
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
          // Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          // Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
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
          // Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          // Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
          continue;
        }

        SelectableLevel[] levels = MrovLib.StringResolver.ResolveStringToLevels(rarityData[0]);

        foreach (SelectableLevel level in levels)
        {
          if (level == null)
          {
            // Plugin.logger.LogWarning($"Invalid level name: {rarityData[0]}");
            continue;
          }

          output.Add(new LevelRarity { Level = level, Weight = weight });
        }
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
          // Plugin.logger.LogWarning($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          // Plugin.logger.LogWarning($"Invalid rarity weight: {rarityData[1]} - not a number!");
          continue;
        }

        Weather weather = ResolveStringToWeather(rarityData[0]);

        if (weather == null)
        {
          // Plugin.logger.LogWarning($"Invalid weather name: {rarityData[0]}");
          continue;
        }

        output.Add(new WeatherRarity { Weather = weather, Weight = weight });
      }

      return output.ToArray();
    }
  }
}
