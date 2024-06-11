using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  public enum WeatherType
  {
    Clear,
    Vanilla,
    Modded,
  }

  public enum WeatherOrigin
  {
    Vanilla,
    WeatherRegistry,
    LethalLib,
    LethalLevelLoader
  }

  public enum FilteringOption
  {
    Include,
    Exclude,
  }

  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather", menuName = "WeatherRegistry/WeatherDefinition", order = 5)]
  public class Weather : ScriptableObject
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType { get; internal set; } = LevelWeatherType.None;

    [JsonIgnore]
    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherRegistry;

    [JsonProperty]
    public WeatherType Type { get; internal set; } = WeatherType.Modded;

    [JsonIgnore]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    [JsonIgnore]
    public ImprovedWeatherEffect Effect;

    [field: SerializeField]
    public float ScrapAmountMultiplier { get; set; } = 1;

    [field: SerializeField]
    public float ScrapValueMultiplier { get; set; } = 1;

    [field: SerializeField]
    [JsonIgnore]
    public string[] DefaultLevelFilters { get; set; } = ["Gordion"];

    [JsonIgnore]
    public List<SelectableLevel> LevelFilters { get; internal set; } = [];

    [field: SerializeField]
    [JsonIgnore]
    public FilteringOption LevelFilteringOption { get; set; } = FilteringOption.Exclude;

    internal ConfigEntry<string> _filterConfig { get; private set; }

    [field: SerializeField]
    public Color Color { get; set; } = Color.cyan;

    [JsonIgnore]
    internal ConfigEntry<int> _defaultWeightConfig { get; private set; }

    [JsonIgnore]
    internal ConfigEntry<bool> _filteringOptionConfig { get; private set; }

    [field: SerializeField]
    public int DefaultWeight { get; set; } = 100;

    [field: SerializeField]
    public Dictionary<LevelWeatherType, int> WeatherWeights { get; set; } = [];

    [JsonIgnore]
    public AnimationClip AnimationClip;

    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      Plugin.logger.LogDebug($"Called Weather constructor for weather {name}");

      Name = name;
      Effect = effect;

      this.name = name;

      if (effect != null)
      {
        Effect.name = name;
      }

      // {(this.Origin != WeatherOrigin.Vanilla ? $"({this.Origin})" : "")}
    }

    internal virtual void Init()
    {
      string configCategory = $"Weather: {name}{(this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

      this._defaultWeightConfig = ConfigManager.configFile.Bind(
        configCategory,
        $"Default weight",
        DefaultWeight,
        new ConfigDescription("The default weight of this weather", new AcceptableValueRange<int>(0, 10000))
      );

      if (DefaultWeight != _defaultWeightConfig.Value)
      {
        this.DefaultWeight = _defaultWeightConfig.Value;
      }

      this._filteringOptionConfig = ConfigManager.configFile.Bind(
        configCategory,
        $"Filtering option",
        this.LevelFilteringOption == FilteringOption.Include,
        new ConfigDescription("Whether to make the filter a whitelist (false is blacklist, true is whitelist)", null)
      );

      this.LevelFilteringOption = _filteringOptionConfig.Value ? FilteringOption.Include : FilteringOption.Exclude;

      this._filterConfig = ConfigManager.configFile.Bind(
        configCategory,
        $"Level filter",
        $"{String.Join(";", DefaultLevelFilters)};",
        new ConfigDescription("Semicolon-separated list of level names to filter", null)
      );

      this.LevelFilters = ConfigHelper.ConvertStringToLevels(_filterConfig.Value).ToList();
    }

    void Reset()
    {
      Type = WeatherType.Modded;
      ScrapAmountMultiplier = 1;
      ScrapValueMultiplier = 1;
      DefaultWeight = 50;
    }

    public void RemoveFromMoon(string moonNames)
    {
      ConfigHelper.ConvertStringToLevels(moonNames).ToList().ForEach(level => LevelFilters.Remove(level));
    }

    public void RemoveFromMoon(SelectableLevel moon)
    {
      LevelFilters.Remove(moon);
    }
  }

  public class LevelWeatherVariables
  {
    public SelectableLevel Level;

    public int WeatherVariable1;
    public int WeatherVariable2;
  }

  public class LevelWeather : LevelWeatherVariables
  {
    public Weather Weather;
    public LevelWeatherVariables Variables;
  }
}
