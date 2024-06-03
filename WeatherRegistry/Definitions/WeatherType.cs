using System;
using System.Collections.Generic;
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

  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather", menuName = "WeatherRegistry/WeatherDefinition", order = 5)]
  public class Weather : ScriptableObject
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType { get; set; } = LevelWeatherType.None;

    [JsonIgnore]
    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherRegistry;

    [JsonProperty]
    public WeatherType Type { get; set; } = WeatherType.Modded;

    [JsonIgnore]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    [JsonIgnore]
    public ImprovedWeatherEffect Effect;

    [field: SerializeField]
    public float ScrapAmountMultiplier { get; set; } = 1;

    [field: SerializeField]
    public float ScrapValueMultiplier { get; set; } = 1;

    [field: SerializeField]
    public List<string> LevelBlacklist { get; set; } = [];

    [JsonIgnore]
    [field: SerializeField]
    public Color Color { get; set; } = Color.cyan;

    [field: SerializeField]
    public int DefaultWeight { get; set; } = 100;

    [field: SerializeField]
    public Dictionary<LevelWeatherType, int> WeatherWeights { get; set; } = [];

    [JsonIgnore]
    public AnimationClip AnimationClip;

    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      Name = name;
      Effect = effect;

      this.name = name;

      Settings.ScreenMapColors.Add(this.Name, this.Color);
    }

    void Reset()
    {
      Type = WeatherType.Modded;
      ScrapAmountMultiplier = 1;
      ScrapValueMultiplier = 1;
      DefaultWeight = 50;
    }

    public static Weather CreateCanvasWeather()
    {
      return new Weather()
      {
        Type = WeatherType.Clear,
        Color = Defaults.VanillaWeatherColors[LevelWeatherType.None],
        VanillaWeatherType = LevelWeatherType.None,
        Origin = WeatherOrigin.Vanilla,
      };
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
