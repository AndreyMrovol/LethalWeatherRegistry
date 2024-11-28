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
    internal BooleanConfigHandler FilteringOption = new(Defaults.FilteringOption == WeatherRegistry.FilteringOption.Include);

    [JsonIgnore]
    internal LevelListConfigHandler LevelFilters = new($"{String.Join(';', Defaults.DefaultLevelFilters)};");

    [JsonIgnore]
    internal LevelWeightsConfigHandler LevelWeights = new($"{String.Join(';', Defaults.DefaultLevelWeights)};");

    [JsonIgnore]
    internal WeatherWeightsConfigHandler WeatherToWeatherWeights = new($"{String.Join(';', Defaults.DefaultWeatherToWeatherWeights)};");

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

      FilteringOption.SetConfigEntry(
        weather,
        "Filtering option",
        new ConfigDescription("Whether to make the filter a whitelist (false is blacklist, true is whitelist)")
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
