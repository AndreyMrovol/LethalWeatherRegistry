using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Modules
{
  public class RegistryWeatherConfig
  {
    [JsonIgnore]
    internal IntegerConfigHandler DefaultWeight = new(Defaults.DefaultWeight);

    [JsonIgnore]
    internal FloatConfigHandler ScrapAmountMultiplier = new(Defaults.ScrapAmountMultiplier);

    [JsonIgnore]
    internal FloatConfigHandler ScrapValueMultiplier = new(Defaults.ScrapValueMultiplier);

    [JsonIgnore]
    internal ConfigEntry<bool> _filteringOptionConfig;

    [JsonIgnore]
    internal LevelListConfigHandler LevelFilters = new(Defaults.DefaultLevelFilters);

    [JsonIgnore]
    internal LevelWeightsConfigHandler LevelWeights = new(Defaults.DefaultLevelWeights);

    [JsonIgnore]
    internal WeatherWeightsConfigHandler WeatherToWeatherWeights = new(Defaults.DefaultWeatherToWeatherWeights);

    public void Init(Weather weather)
    {
      DefaultWeight.SetConfigEntry(
        weather,
        "Default weight",
        new ConfigDescription("The default weight of this weather", new AcceptableValueRange<int>(0, 10000))
      );

      ScrapAmountMultiplier.SetConfigEntry(
        weather,
        "Scrap amount multiplier",
        new ConfigDescription("Multiplier for the amount of scrap spawned", new AcceptableValueRange<float>(0, 100))
      );

      ScrapValueMultiplier.SetConfigEntry(
        weather,
        "Scrap value multiplier",
        new ConfigDescription("Multiplier for the value of scrap spawned", new AcceptableValueRange<float>(0, 100))
      );

      this._filteringOptionConfig = ConfigManager.configFile.Bind(
        weather.ConfigCategory,
        $"Filtering option",
        weather.LevelFilteringOption == FilteringOption.Include,
        "Whether to include or exclude the levels in the list below"
      );

      LevelFilters.SetConfigEntry(weather, "Level filter", new ConfigDescription("Semicolon-separated list of level names to filter", null));

      LevelWeights.SetConfigEntry(weather, "Level weights", new ConfigDescription("Semicolon-separated list of level weights", null));

      WeatherToWeatherWeights.SetConfigEntry(
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
