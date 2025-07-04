using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;
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
    LethalLevelLoader,
    WeatherTweaks,
  }

  public enum FilteringOption
  {
    Include,
    Exclude,
  }

  [JsonObject(MemberSerialization.OptIn)]
  // [CreateAssetMenu(fileName = "Weather", menuName = "WeatherRegistry/OlderWeatherDefinition", order = 5)]
  public class Weather : ScriptableObject
  {
    #region Base properties

    [JsonProperty]
    public string Name;

    [JsonIgnore]
    public ImprovedWeatherEffect Effect;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType { get; internal set; } = LevelWeatherType.None;

    [JsonIgnore]
    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherRegistry;

    [JsonProperty]
    public WeatherType Type { get; internal set; } = WeatherType.Modded;

    [JsonIgnore]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    [JsonIgnore]
    [Obsolete]
    public AnimationClip AnimationClip;

    [field: SerializeField]
    public Color Color { get; set; } = Color.cyan;

    [JsonIgnore]
    public RegistryWeatherConfig Config = new();

    [JsonIgnore]
    internal Dictionary<SelectableLevel, Definitions.WeatherEffectOverride> WeatherEffectOverrides = [];

    #endregion

    #region backing fields

    [Obsolete("Use Weather.Config.DefaultWeight instead")]
    internal int _defaultWeight = 100;

    [Obsolete("Use Weather.Config.ScrapAmountMultiplier instead")]
    internal float _scrapAmountMultiplier = 1;

    [Obsolete("Use Weather.Config.ScrapValueMultiplier instead")]
    internal float _scrapValueMultiplier = 1;

    #endregion

    #region defaults

    [property: SerializeField]
    public int DefaultWeight
    {
      get { return Config.DefaultWeight.Value; }
      [Obsolete("Use Weather.Config.DefaultWeight instead")]
      set { _defaultWeight = value; }
    }

    [Obsolete("Use Weather.Config.LevelFilters instead")]
    public string[] DefaultLevelFilters
    {
      get { return this.Config.LevelFilters.DefaultValue.Split(";"); }
      set { this.Config.LevelFilters = new(value); }
    }

    [Obsolete("Use Weather.Config.LevelWeights instead")]
    public string[] DefaultLevelWeights
    {
      get { return this.Config.LevelWeights.DefaultValue.Split(";"); }
      set { this.Config.LevelWeights = new(value); }
    }

    [Obsolete("Use Weather.Config.WeatherToWeatherWeights instead")]
    public string[] DefaultWeatherToWeatherWeights
    {
      get { return this.Config.WeatherToWeatherWeights.DefaultValue.Split(";"); }
      set { this.Config.WeatherToWeatherWeights = new(value); }
    }

    public float ScrapAmountMultiplier
    {
      get { return Config.ScrapAmountMultiplier.Value; }
      [Obsolete("Use Weather.Config.ScrapAmountMultiplier instead")]
      set { Config.ScrapAmountMultiplier = new(value); }
    }

    public float ScrapValueMultiplier
    {
      get { return Config.ScrapValueMultiplier.Value; }
      [Obsolete("Use Weather.Config.ScrapValueMultiplier instead")]
      set { Config.ScrapValueMultiplier = new(value); }
    }

    #endregion

    #region stuff from config

    [property: SerializeField]
    [JsonIgnore]
    public FilteringOption LevelFilteringOption
    {
      get { return Config.FilteringOption.Value ? FilteringOption.Include : FilteringOption.Exclude; }
      [Obsolete("Use Weather.Config.FilteringOption instead")]
      set { Config.FilteringOption = new(value == FilteringOption.Include); }
    }

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

    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      Plugin.logger.LogDebug($"Called Weather constructor for weather {name}");

      // a small hack for Whimsical weather so it doesn't use <color> tags in their name
      Regex textTagsRegex = new(@"<.*?>");
      Name = textTagsRegex.Replace(name, "");
      this.name = textTagsRegex.Replace(name, "");

      Effect = effect;

      if (effect != null)
      {
        Effect.name = name;
      }

      GameObject.DontDestroyOnLoad(this);
      // GameObject.Instantiate(this);
    }

    #endregion

    internal virtual string ConfigCategory =>
      $"{(this.Type == WeatherType.Vanilla || this.Type == WeatherType.Clear ? "Vanilla" : "Modded")} Weather: {this.name.Replace(" ", "")}{(this.Origin != WeatherOrigin.WeatherRegistry && this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

    internal virtual void Init()
    {
      this.Config.Init(this);

      if (this.Effect != null)
      {
        this.Effect.LevelWeatherType = this.VanillaWeatherType;
      }

      this.hideFlags = HideFlags.HideAndDontSave;
    }

    // void Reset()
    // {
    //   Type = WeatherType.Modded;
    //   ScrapAmountMultiplier = 1;
    //   ScrapValueMultiplier = 1;
    //   DefaultWeight = 50;
    // }

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

        // this shit is in dire need of a rework
        // currently the system works in reverse:
        // `None@200` defined in Rainy means that if None was the previous weather, Rainy is set to 200 weight
        // what a dumb motherfucking system

        logger.LogDebug($"{this.Name} has weather>weather weight {weatherWeightFromWeather}");
        weatherWeight = weatherWeightFromWeather;
      }
      else
      {
        logger.LogDebug($"{this.Name} has default weight {weatherWeight}");
      }

      return weatherWeight;
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
