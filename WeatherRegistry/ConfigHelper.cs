using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using HarmonyLib;

namespace WeatherRegistry
{
  public class ConfigHelper
  {
    private static MrovLib.Logger logger = new("WeatherRegistry", ConfigManager.LogWeightResolving);

    private static readonly Regex ConfigCleanerRegex = new(@"[\n\t""`\[\]']");

    internal static string CleanStringForConfig(string input)
    {
      // The regex pattern matches: newline, tab, double quote, backtick, apostrophe, [ or ].
      return ConfigCleanerRegex.Replace(input, string.Empty).Trim();
    }

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

    public static List<Weather> ResolveStringToWeathers(string str)
    {
      if (string.IsNullOrWhiteSpace(str))
      {
        return [];
      }

      string[] weatherNames = ConvertStringToArray(str);
      List<Weather> weathers = [];

      foreach (string weatherName in weatherNames)
      {
        Weather weather = ResolveStringToWeather(weatherName);

        if (weather != null)
        {
          weathers.Add(weather);
        }
        else
        {
          logger.LogWarning($"Invalid weather name: {weatherName}");
        }
      }

      return weathers;
    }

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
      Dictionary<string, int> output = [];

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

        string name = rarityData[0].Trim();

        if (output.ContainsKey(name))
        {
          // logger.LogWarning($"Duplicate key: {name}");
          continue;
        }

        output.Add(name, weight);
      }

      return output.Select(rarity => new NameRarity { Name = rarity.Key, Weight = rarity.Value }).ToArray();
    }

    public static LevelRarity[] ConvertStringToLevelRarities(string str)
    {
      // i want to use the following format:
      // LevelName@Weight;LevelName@Weight;LevelName@Weight

      Dictionary<SelectableLevel, int> output = [];
      NameRarity[] nameRarities = ConvertStringToRarities(str);

      foreach (NameRarity nameRarity in nameRarities)
      {
        SelectableLevel[] levels = MrovLib.StringResolver.ResolveStringToLevels(nameRarity.Name);

        foreach (SelectableLevel level in levels)
        {
          if (level == null)
          {
            // Plugin.logger.LogWarning($"Invalid level name: {rarityData[0]}");
            continue;
          }

          output.TryAdd(level, nameRarity.Weight);
        }
      }

      return output.Select(rarity => new LevelRarity { Level = rarity.Key, Weight = rarity.Value }).ToArray();
    }

    public static WeatherRarity[] ConvertStringToWeatherWeights(string str)
    {
      // i want to use the following format:
      // Weather@Weight;Weather@Weight;Weather@Weight

      Dictionary<Weather, int> output = [];
      NameRarity[] nameRarities = ConvertStringToRarities(str);

      foreach (NameRarity nameRarity in nameRarities)
      {
        Weather weather = ResolveStringToWeather(nameRarity.Name);

        if (weather == null)
        {
          // logger.LogWarning($"Invalid weather name: {nameRarity.Name}");
          continue;
        }

        output.TryAdd(weather, nameRarity.Weight);
      }

      return output.Select(rarity => new WeatherRarity { Weather = rarity.Key, Weight = rarity.Value }).ToArray();
    }
  }
}
