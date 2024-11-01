using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Patches;

namespace WeatherRegistry.Modules
{
  // [JsonObject(MemberSerialization.OptIn)]
  // [CreateAssetMenu(fileName = "Weather", menuName = "WeatherRegistry/WeatherDefinition", order = 5)]
  public class RegistryWeather : ScriptableObject
  {
    #region Base properties

    [JsonProperty]
    public string Name { get; }

    [JsonIgnore]
    public RegistryWeatherEffect Effect { get; }

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType { get; internal set; } = LevelWeatherType.None;

    [JsonIgnore]
    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherRegistry;

    [JsonProperty]
    public WeatherType Type { get; internal set; } = WeatherType.Modded;

    [JsonIgnore]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    [JsonIgnore]
    public AnimationClip AnimationClip { get; set; }

    [field: SerializeField]
    public Color Color { get; } = Color.cyan;

    [JsonIgnore]
    public RegistryWeatherConfig Config { get; set; } = new();

    [JsonIgnore]
    internal Dictionary<SelectableLevel, WeatherEffectOverride> WeatherEffectOverrides = [];

    #endregion

    #region variables from config

    [property: SerializeField]
    public int DefaultWeight
    {
      get { return Config.DefaultWeight.Value; }
    }

    [property: SerializeField]
    public float ScrapAmountMultiplier
    {
      get { return Config.ScrapAmountMultiplier.Value; }
    }

    [property: SerializeField]
    public float ScrapValueMultiplier
    {
      get { return Config.ScrapValueMultiplier.Value; }
    }

    [field: SerializeField]
    [JsonIgnore]
    public FilteringOption LevelFilteringOption { get; set; } = FilteringOption.Exclude;

    [JsonIgnore]
    public List<SelectableLevel> LevelFilters
    {
      get { return Config.LevelFilters.Value.ToList(); }
    }

    [JsonIgnore]
    public Dictionary<LevelWeatherType, int> WeatherWeights
    {
      get { return Config.WeatherToWeatherWeights.Value.ToDictionary(rarity => rarity.Weather.VanillaWeatherType, rarity => rarity.Weight); }
    }

    [JsonIgnore]
    public Dictionary<SelectableLevel, int> LevelWeights
    {
      get { return Config.LevelWeights.Value.ToDictionary(rarity => rarity.Level, rarity => rarity.Weight); }
    }

    #endregion

    #region Constructor

    internal RegistryWeather(WeatherDefinition weatherDefinition)
    {
      Plugin.logger.LogDebug($"Called Weather constructor for weather {weatherDefinition.Name}");

      // a small hack for Whimsical weather so it doesn't use <color> tags in their name
      Regex textTagsRegex = new(@"<.*?>");
      Name = textTagsRegex.Replace(weatherDefinition.Name, "");
      this.name = textTagsRegex.Replace(weatherDefinition.Name, "");

      Effect = new(weatherDefinition.Effect);

      if (weatherDefinition.Effect != null)
      {
        Effect.name = name;
      }

      Color = weatherDefinition.Color;
      AnimationClip = weatherDefinition.AnimationClip;
      Config = weatherDefinition.Configuration;

      GameObject.DontDestroyOnLoad(this);
      GameObject.Instantiate(this);

      WeatherManager.Weathers.Add(this);
    }

    [Obsolete]
    internal RegistryWeather(string name = "None", ImprovedWeatherEffect effect = default)
      : this(
        new()
        {
          Name = name,
          Effect = new(effect),
          AnimationClip = null,
          Color = Color.cyan,
          Configuration = new RegistryWeatherConfig() { }
        }
      ) { }

    internal virtual void Init()
    {
      this.Config.Init(this);

      this.LevelFilteringOption = Config._filteringOptionConfig.Value ? FilteringOption.Include : FilteringOption.Exclude;

      this.hideFlags = HideFlags.HideAndDontSave;
    }

    #endregion

    internal string ConfigCategory =>
      $"{(this.Type == WeatherType.Vanilla || this.Type == WeatherType.Clear ? "Vanilla" : "Modded")} Weather: {this.name}{(this.Origin != WeatherOrigin.WeatherRegistry && this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

    public void RemoveFromMoon(string moonNames)
    {
      ConfigHelper.ConvertStringToLevels(moonNames).ToList().ForEach(level => LevelFilters.Remove(level));
    }

    public void RemoveFromMoon(SelectableLevel moon)
    {
      LevelFilters.Remove(moon);
    }

    public int GetWeight(SelectableLevel level)
    {
      MrovLib.Logger logger = WeatherCalculation.Logger;
      var weatherWeight = this.DefaultWeight;

      var previousWeather = WeatherManager.GetWeather(level.currentWeather);

      // we have 3 weights possible:
      // 1. level weight
      // 2. weather-weather weights
      // 3. default weight
      // we want to execute them in this exact order

      if (this.LevelWeights.TryGetValue(level, out int levelWeight))
      {
        // (1) => level weight
        logger.LogDebug($"{this.Name} has level weight {levelWeight}");
        weatherWeight = levelWeight;
      }
      // try to get previous day weather (so - at this point - the current one)
      // but not on first day because that's completely random
      else if (
        previousWeather.WeatherWeights.TryGetValue(this.VanillaWeatherType, out int weatherWeightFromWeather)
        && StartOfRound.Instance.gameStats.daysSpent != 0
      )
      {
        // (2) => weather-weather weights

        logger.LogDebug($"{this.Name} has weather>weather weight {weatherWeightFromWeather}");
        weatherWeight = weatherWeightFromWeather;
      }
      else
      {
        logger.LogDebug($"{this.Name} has default weight {weatherWeight}");
      }

      return weatherWeight;
    }

    public (bool isWTW, int weight) GetWeatherToWeatherWeight(RegistryWeather previousWeather)
    {
      if (previousWeather.WeatherWeights.TryGetValue(this.VanillaWeatherType, out int weatherWeightFromWeather))
      {
        return (true, weatherWeightFromWeather);
      }

      return (false, DefaultWeight);
    }

    public WeatherEffectOverride GetEffectOverride(SelectableLevel level)
    {
      if (WeatherEffectOverrides.ContainsKey(level))
      {
        return WeatherEffectOverrides[level];
      }

      return null;
    }
  }
}
