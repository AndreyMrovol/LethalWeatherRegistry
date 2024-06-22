using System;
using System.Collections.Generic;
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
      string configCategory = $"Weather: {weather.name}{(weather.Origin != WeatherOrigin.Vanilla ? $" ({weather.Origin})" : "")}";

      // this._defaultWeightConfig = ConfigManager.configFile.Bind(
      //   configCategory,
      //   $"Default weight",
      //   weather.DefaultWeight,
      //   new ConfigDescription("The default weight of this weather", new AcceptableValueRange<int>(0, 10000))
      // );

      DefaultWeight = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Default weight",
          weather._defaultWeight,
          new ConfigDescription("The default weight of this weather", new AcceptableValueRange<int>(0, 10000))
        ),
        DefaultValue = weather._defaultWeight,
      };

      ScrapAmountMultiplier = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Scrap amount multiplier",
          weather._scrapAmountMultiplier,
          new ConfigDescription("Multiplier for the amount of scrap spawned", new AcceptableValueRange<float>(0, 10000))
        ),
        DefaultValue = weather._scrapAmountMultiplier,
      };

      ScrapValueMultiplier = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Scrap value multiplier",
          weather._scrapValueMultiplier,
          new ConfigDescription("Multiplier for the value of scrap spawned", new AcceptableValueRange<float>(0, 10000))
        ),
        DefaultValue = weather._scrapValueMultiplier,
      };

      this._filteringOptionConfig = ConfigManager.configFile.Bind(
        configCategory,
        $"Filtering option",
        weather.LevelFilteringOption == FilteringOption.Include,
        new ConfigDescription("Whether to make the filter a whitelist (false is blacklist, true is whitelist)", null)
      );

      LevelFilters = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Level filter",
          $"{String.Join(";", weather.DefaultLevelFilters)};",
          new ConfigDescription("Semicolon-separated list of level names to filter", null)
        ),
        DefaultValue = $"{String.Join(";", weather.DefaultLevelFilters)};",
      };

      LevelWeights = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Level weights",
          $"LevelName@Weight;",
          new ConfigDescription("Semicolon-separated list of level weights", null)
        ),
        DefaultValue = "",
      };

      WeatherToWeatherWeights = new()
      {
        _configEntry = ConfigManager.configFile.Bind(
          configCategory,
          $"Weather weights",
          $"WeatherName@Weight;",
          new ConfigDescription("Semicolon-separated list of weather weights", null)
        ),
        DefaultValue = "",
      };
    }
  }
}
