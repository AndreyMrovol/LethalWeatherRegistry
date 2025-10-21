using System;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace WeatherRegistry.Modules
{
  public class RegistryWeatherConfig
  {
    public IntegerConfigHandler DefaultWeight = new(Defaults.DefaultWeight);

    public FloatConfigHandler ScrapAmountMultiplier = new(Defaults.ScrapAmountMultiplier);

    public FloatConfigHandler ScrapValueMultiplier = new(Defaults.ScrapValueMultiplier);

    public BooleanConfigHandler FilteringOption = new(Defaults.FilteringOption);

    public LevelListConfigHandler LevelFilters = new($"{String.Join(';', Defaults.DefaultLevelFilters)};");

    public LevelWeightsConfigHandler LevelWeights = new($"{String.Join(';', Defaults.DefaultLevelWeights)};");

    public WeatherWeightsConfigHandler WeatherToWeatherWeights = new($"{String.Join(';', Defaults.DefaultWeatherToWeatherWeights)};");

    public virtual void Init(ImprovedWeather weather)
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
