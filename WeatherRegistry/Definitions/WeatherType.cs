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

    [JsonIgnore]
    public List<SelectableLevel> LevelFilters { get; internal set; }

    [field: SerializeField]
    [JsonIgnore]
    public FilteringOption LevelFilteringOption { get; set; } = FilteringOption.Exclude;

    [field: SerializeField]
    public Dictionary<LevelWeatherType, int> WeatherWeights;

    [field: SerializeField]
    public Dictionary<SelectableLevel, int> LevelWeights;

    #endregion

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

      this.Config.Init(this);

      this.WeatherWeights = Config.WeatherToWeatherWeights.Value.ToDictionary(
        rarity => rarity.Weather.VanillaWeatherType,
        rarity => rarity.Weight
      );
      this.LevelWeights = Config.LevelWeights.Value.ToDictionary(rarity => rarity.Level, rarity => rarity.Weight);

      this.LevelFilteringOption = Config._filteringOptionConfig.Value ? FilteringOption.Include : FilteringOption.Exclude;
      this.LevelFilters = Config.LevelFilters.Value.ToList();

      this.hideFlags = HideFlags.HideAndDontSave;
      GameObject.DontDestroyOnLoad(this);
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
