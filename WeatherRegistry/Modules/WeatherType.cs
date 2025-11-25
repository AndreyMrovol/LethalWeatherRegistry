using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WeatherRegistry.Enums;
using WeatherRegistry.Helpers;
using WeatherRegistry.Modules;

namespace WeatherRegistry
{
  public class Weather
  {
    #region Base properties

    public string Name { get; set; }

    [Obsolete("Use Weather.Name instead")]
    public string name => Name;

    public ImprovedWeatherEffect Effect { get; internal set; }

    public LevelWeatherType VanillaWeatherType { get; internal set; } = LevelWeatherType.None;

    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherRegistry;

    public WeatherType Type { get; internal set; } = WeatherType.Modded;

    [field: SerializeField]
    public TMP_ColorGradient Color { get; set; } = ColorHelper.ToTMPColorGradient(UnityEngine.Color.cyan);
    public TMP_ColorGradient Colour => Color;

    public RegistryWeatherConfig Config = new();

    internal Dictionary<SelectableLevel, Definitions.WeatherEffectOverride> WeatherEffectOverrides = [];

    [Obsolete]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    #endregion

    #region defaults (all obsolete)
    public int DefaultWeight => Config.DefaultWeight.Value;

    [Obsolete("Use Weather.Config.LevelFilters instead")]
    public string[] DefaultLevelFilters => this.Config.LevelFilters.DefaultValue.Split(";");

    [Obsolete("Use Weather.Config.LevelWeights instead")]
    public string[] DefaultLevelWeights => this.Config.LevelWeights.DefaultValue.Split(";");

    [Obsolete("Use Weather.Config.WeatherToWeatherWeights instead")]
    public string[] DefaultWeatherToWeatherWeights => this.Config.WeatherToWeatherWeights.DefaultValue.Split(";");

    [Obsolete("Use Weather.Config.ScrapAmountMultiplier instead")]
    public float ScrapAmountMultiplier => Config.ScrapAmountMultiplier.Value;

    [Obsolete("Use Weather.Config.ScrapAmountMultiplier instead")]
    public float ScrapValueMultiplier => Config.ScrapValueMultiplier.Value;

    #endregion

    #region stuff from config

    [property: SerializeField]
    public FilteringOption LevelFilteringOption
    {
      get { return Config.FilteringOption.Value ? FilteringOption.Include : FilteringOption.Exclude; }
      [Obsolete("Use Weather.Config.FilteringOption instead")]
      set { Config.FilteringOption = new(value == FilteringOption.Include); }
    }

    public List<SelectableLevel> LevelFilters
    {
      get { return Config.LevelFilters.Value.ToList(); }
    }

    public Dictionary<LevelWeatherType, int> WeatherWeights
    {
      get { return Config.WeatherToWeatherWeights.Value.ToDictionary(rarity => rarity.Weather.VanillaWeatherType, rarity => rarity.Weight); }
    }

    public Dictionary<SelectableLevel, int> LevelWeights
    {
      get { return Config.LevelWeights.Value.ToDictionary(rarity => rarity.Level, rarity => rarity.Weight); }
    }

    #endregion

    #region Constructor

    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      // a small hack for Whimsical weather so it doesn't use <color> tags in their name
      Regex textTagsRegex = new(@"<.*?>");
      Name = textTagsRegex.Replace(name, "");

      Effect = effect;

      if (effect != null)
      {
        Effect.name = Name;
      }
    }

    #endregion

    internal virtual string ConfigCategory =>
      $"{(this.Type == WeatherType.Vanilla || this.Type == WeatherType.Clear ? "Vanilla" : "Modded")} Weather: {this.Name.Replace(" ", "")}{(this.Origin != WeatherOrigin.WeatherRegistry && this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

    internal virtual void Init()
    {
      this.Config.Init(this);

      if (this.Effect != null)
      {
        this.Effect.LevelWeatherType = this.VanillaWeatherType;
      }

      this.Color.name = $"Weather{this.GetAlphanumericName()}";

      // this.name = Name;
      // this.effectObject = Effect?.EffectObject;
      // this.effectPermanentObject = Effect?.WorldObject;
    }

    #region Miscellaneous methods

    public override string ToString()
    {
      return Name;
    }

    public void RemoveFromMoon(string moonNames)
    {
      ConfigHelper.ConvertStringToLevels(moonNames).ToList().ForEach(level => LevelFilters.Remove(level));
    }

    public void RemoveFromMoon(SelectableLevel moon)
    {
      LevelFilters.Remove(moon);
    }

    public (int weight, WeatherWeightType type) GetWeightWithOrigin(SelectableLevel level)
    {
      Logger logger = WeatherCalculation.Logger;
      var weatherWeight = this.DefaultWeight;
      WeatherWeightType weatherWeightType = WeatherWeightType.Default;

      var previousWeather = WeatherManager.GetWeather(level.currentWeather);

      if (previousWeather == null)
      {
        logger.LogError($"Previous weather is null for {level.name}");
      }

      // we have 3 weights possible:
      // 1. level weight
      // 2. weather-weather weights
      // 3. default weight
      // we want to execute them in this exact order

      if (this.LevelWeights.TryGetValue(level, out int levelWeight))
      {
        // (1) => level weight
        // logger.LogDebug($"{this.Name} has level weight {levelWeight}");
        weatherWeightType = WeatherWeightType.Level;
        weatherWeight = levelWeight;
      }
      // try to get previous day weather (so - at this point - the current one)
      // but not on first day because that's unreliable and random (from my testing)
      else if (
        previousWeather.WeatherWeights.TryGetValue(this.VanillaWeatherType, out int weatherWeightFromWeather)
        && StartOfRound.Instance.gameStats.daysSpent != 0
      )
      {
        // (2) => weather-weather weights

        // logger.LogDebug($"{this.Name} has weather>weather weight {weatherWeightFromWeather}");
        weatherWeightType = WeatherWeightType.WeatherToWeather;
        weatherWeight = weatherWeightFromWeather;
      }
      else
      {
        weatherWeightType = WeatherWeightType.Default;
        // logger.LogDebug($"{this.Name} has default weight {weatherWeight}");
      }

      return (weatherWeight, weatherWeightType);
    }

    public int GetWeight(SelectableLevel level)
    {
      return GetWeightWithOrigin(level).weight;
    }

    public string GetAlphanumericName()
    {
      return ConfigHelper.GetAlphanumericName(this);
    }

    public (bool isWTW, int weight) GetWeatherToWeatherWeight(Weather previousWeather)
    {
      if (previousWeather.WeatherWeights.TryGetValue(this.VanillaWeatherType, out int weatherWeightFromWeather))
      {
        return (true, weatherWeightFromWeather);
      }

      return (false, DefaultWeight);
    }

    public Definitions.WeatherEffectOverride GetEffectOverride(SelectableLevel level)
    {
      if (WeatherEffectOverrides.ContainsKey(level))
      {
        return WeatherEffectOverrides[level];
      }

      return null;
    }
  }

    #endregion
}
