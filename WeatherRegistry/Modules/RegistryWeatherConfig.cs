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
    public IntegerConfigHandler DefaultWeight = new(Defaults.DefaultWeight);

    [JsonIgnore]
    public FloatConfigHandler ScrapAmountMultiplier = new(Defaults.ScrapAmountMultiplier);

    [JsonIgnore]
    public FloatConfigHandler ScrapValueMultiplier = new(Defaults.ScrapValueMultiplier);

    [JsonIgnore]
    public BooleanConfigHandler FilteringOption = new(Defaults.FilteringOption);

    [JsonIgnore]
    public LevelListConfigHandler LevelFilters = new($"{String.Join(';', Defaults.DefaultLevelFilters)};");

    [JsonIgnore]
    public LevelWeightsConfigHandler LevelWeights = new($"{String.Join(';', Defaults.DefaultLevelWeights)};");

    [JsonIgnore]
    public WeatherWeightsConfigHandler WeatherToWeatherWeights = new($"{String.Join(';', Defaults.DefaultWeatherToWeatherWeights)};");

    public virtual void Init(Weather weather)
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

      LevelFilters.SetConfigEntry(
        weather,
        "Level filter",
        new ConfigDescription("Semicolon-separated list of level names to filter (use `Filtering Option` config to select filter type)", null)
      );

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
