using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public AnimationClip AnimationClip;

    [field: SerializeField]
    public Color Color { get; set; } = Color.cyan;

    [JsonIgnore]
    internal WeatherConfig Config = new();

    #endregion

    #region backing fields

    internal int _defaultWeight = 100;
    internal float _scrapAmountMultiplier = 1;
    internal float _scrapValueMultiplier = 1;

    #endregion

    #region defaults

    [property: SerializeField]
    public int DefaultWeight
    {
      get { return Config.DefaultWeight.Value; }
      set { _defaultWeight = value; }
    }

    [field: SerializeField]
    [JsonIgnore]
    public string[] DefaultLevelFilters { get; set; } = ["Gordion"];

    public string[] DefaultLevelWeights { get; set; } = ["MoonName@50"];

    public string[] DefaultWeatherToWeatherWeights { get; set; } = ["WeatherName@50"];

    [property: SerializeField]
    public float ScrapAmountMultiplier
    {
      get { return Config.ScrapAmountMultiplier.Value; }
      set { _scrapAmountMultiplier = value; }
    }

    [property: SerializeField]
    public float ScrapValueMultiplier
    {
      get { return Config.ScrapValueMultiplier.Value; }
      set { _scrapValueMultiplier = value; }
    }

    #endregion

    #region stuff from config

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

    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      Plugin.logger.LogDebug($"Called Weather constructor for weather {name}");

      Regex textTagsRegex = new(@"<.*?>");

      Name = textTagsRegex.Replace(name, "");
      this.name = textTagsRegex.Replace(name, "");

      Effect = effect;

      if (effect != null)
      {
        Effect.name = name;
      }

      // {(this.Origin != WeatherOrigin.Vanilla ? $"({this.Origin})" : "")}
    }

    #endregion

    internal virtual void Init()
    {
      string configCategory = $"Weather: {name}{(this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

      this.Config.Init(this);

      this.LevelFilteringOption = Config._filteringOptionConfig.Value ? FilteringOption.Include : FilteringOption.Exclude;

      this.hideFlags = HideFlags.HideAndDontSave;

      GameObject.DontDestroyOnLoad(this);
      GameObject.Instantiate(this);
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

    public (bool isWTW, int weight) GetWeatherToWeatherWeight(Weather previousWeather)
    {
      if (previousWeather.WeatherWeights.TryGetValue(this.VanillaWeatherType, out int weatherWeightFromWeather))
      {
        return (true, weatherWeightFromWeather);
      }

      return (false, DefaultWeight);
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
