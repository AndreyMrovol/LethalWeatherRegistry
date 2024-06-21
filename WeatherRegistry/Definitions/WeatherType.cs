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

    #endregion

    #region defaults

    public int DefaultWeight { get; set; } = 100;

    [field: SerializeField]
    [JsonIgnore]
    public string[] DefaultLevelFilters { get; set; } = ["Gordion"];

    [JsonIgnore]
    internal ConfigEntry<int> _defaultWeightConfig { get; private set; }

    [JsonIgnore]
    internal ConfigEntry<bool> _filteringOptionConfig { get; private set; }

    #endregion

    #region stuff from config

    [field: SerializeField]
    public float ScrapAmountMultiplier { get; set; } = 1;

    [field: SerializeField]
    public float ScrapValueMultiplier { get; set; } = 1;

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



    [JsonIgnore]
    internal WeatherConfig Config;

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

      Config = new WeatherConfig();

      // {(this.Origin != WeatherOrigin.Vanilla ? $"({this.Origin})" : "")}
    }

    internal virtual void Init()
    {
      string configCategory = $"Weather: {name}{(this.Origin != WeatherOrigin.Vanilla ? $" ({this.Origin})" : "")}";

      this.Config.Init(this);

      if (DefaultWeight != _defaultWeightConfig.Value)
      {
        this.DefaultWeight = _defaultWeightConfig.Value;
      }

      this.WeatherWeights = Config.WeatherToWeatherWeights.Value.ToDictionary(
        rarity => rarity.Weather.VanillaWeatherType,
        rarity => rarity.Weight
      );

      this.LevelWeights = Config.LevelWeights.Value.ToDictionary(rarity => rarity.Level, rarity => rarity.Weight);

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
