using System;
using System.Collections.Generic;
using UnityEngine;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Editor
{
  [Serializable]
  public class SerializableConfigElement<T>(T defaultValue, bool isEnabled = true)
  {
    [SerializeField]
    [Tooltip("Whether this config element is user-configurable. If not, only the value set by you will be used.")]
    public bool enabled = isEnabled;

    [SerializeField]
    [Tooltip("Value of the config element.")]
    public T value = defaultValue;
  }

  [Serializable]
  public class SerializableWeatherConfig
  {
    [Header("Filtering options")]
    [SerializeField]
    [Tooltip("Whether to make the filter a whitelist or a blacklist.")]
    public SerializableConfigElement<WeatherRegistry.FilteringOption> filteringOption = new(FilteringOption.Exclude);

    [SerializeField]
    [Tooltip("Semicolon-separated list of level names that this weather applies to. This setting uses `Filtering Option` set above.")]
    public SerializableConfigElement<string> levelFilters = new(String.Join(";", Defaults.DefaultLevelFilters));

    [Header("Scrap multiplier settings")]
    [SerializeField]
    [Tooltip("Multiplier for the amount of scrap spawned (vanilla value is 1.0).")]
    public SerializableConfigElement<float> scrapAmountMultiplier = new(Defaults.ScrapAmountMultiplier);

    [SerializeField]
    [Tooltip("Multiplier for the value of scrap spawned (vanilla value is 1.0).")]
    public SerializableConfigElement<float> scrapValueMultiplier = new(Defaults.ScrapValueMultiplier);

    [Header("Weight settings")]
    [SerializeField]
    [Tooltip("Semicolon-separated list of level weights.")]
    public SerializableConfigElement<string> levelWeights = new(String.Join(";", Defaults.DefaultLevelWeights));

    [SerializeField]
    [Tooltip("Semicolon-separated list of weather-to-weather weights.")]
    public SerializableConfigElement<string> weatherToWeatherWeights = new(String.Join(";", Defaults.DefaultWeatherToWeatherWeights));

    [SerializeField]
    [Tooltip("The default weight of this weather.")]
    public SerializableConfigElement<int> defaultWeight = new(Defaults.DefaultWeight);

    public RegistryWeatherConfig CreateFullConfig()
    {
      return new RegistryWeatherConfig
      {
        FilteringOption = new BooleanConfigHandler(filteringOption.value, filteringOption.enabled),
        LevelFilters = new LevelListConfigHandler(levelFilters.value, levelFilters.enabled),
        ScrapAmountMultiplier = new FloatConfigHandler(scrapAmountMultiplier.value, scrapAmountMultiplier.enabled),
        ScrapValueMultiplier = new FloatConfigHandler(scrapValueMultiplier.value, scrapValueMultiplier.enabled),
        LevelWeights = new LevelWeightsConfigHandler(levelWeights.value, levelWeights.enabled),
        WeatherToWeatherWeights = new WeatherWeightsConfigHandler(weatherToWeatherWeights.value, weatherToWeatherWeights.enabled),
        DefaultWeight = new IntegerConfigHandler(defaultWeight.value, defaultWeight.enabled)
      };
    }
  }
}
