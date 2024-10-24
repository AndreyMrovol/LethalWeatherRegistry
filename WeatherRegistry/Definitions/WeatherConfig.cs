using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace WeatherRegistry
{
  public class WeatherConfig
  {
    [JsonIgnore]
    internal IntegerConfigHandler DefaultWeight;

    [JsonIgnore]
    internal FloatConfigHandler ScrapAmountMultiplier;

    [JsonIgnore]
    internal FloatConfigHandler ScrapValueMultiplier;

    [JsonIgnore]
    internal ConfigEntry<bool> _filteringOptionConfig { get; private set; }

    [JsonIgnore]
    internal LevelListConfigHandler LevelFilters;

    [JsonIgnore]
    internal LevelWeightsConfigHandler LevelWeights;

    [JsonIgnore]
    internal WeatherWeightsConfigHandler WeatherToWeatherWeights;

    internal void Init(Weather weather)
    {
      DefaultWeight = new(
        weather._defaultWeight,
        weather,
        "Default weight",
        new ConfigDescription("The default weight of this weather", new AcceptableValueRange<int>(0, 10000))
      );

      ScrapAmountMultiplier = new(
        weather._scrapAmountMultiplier,
        weather,
        "Scrap amount multiplier",
        new ConfigDescription("Multiplier for the amount of scrap spawned", new AcceptableValueRange<float>(0, 100))
      );

      ScrapValueMultiplier = new(
        weather._scrapValueMultiplier,
        weather,
        "Scrap value multiplier",
        new ConfigDescription("Multiplier for the value of scrap spawned", new AcceptableValueRange<float>(0, 100))
      );

      this._filteringOptionConfig = ConfigManager.configFile.Bind(
        weather.ConfigCategory,
        $"Filtering option",
        weather.LevelFilteringOption == FilteringOption.Include,
        new ConfigDescription("Whether to make the filter a whitelist (false is blacklist, true is whitelist)", null)
      );

      LevelFilters = new(
        $"{String.Join(";", weather.DefaultLevelFilters)};",
        weather,
        "Level filter",
        new ConfigDescription("Semicolon-separated list of level names to filter", null)
      );

      LevelWeights = new(
        $"{String.Join(';', weather.DefaultLevelWeights)};",
        weather,
        "Level weights",
        new ConfigDescription("Semicolon-separated list of level weights", null)
      );

      WeatherToWeatherWeights = new(
        $"{(Defaults.VanillaWeatherToWeatherWeights.TryGetValue(weather.VanillaWeatherType, out string weights) ? weights : $"{String.Join(';', weather.DefaultWeatherToWeatherWeights)};")}",
        weather,
        "WeatherToWeather weights",
        new ConfigDescription(
          $"Semicolon-separated list of weather-to-weather weights - if previous day was {weather.Name}, next day should have weights:",
          null
        )
      );
    }
  }
}
