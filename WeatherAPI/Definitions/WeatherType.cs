using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using WeatherAPI.Patches;

namespace WeatherAPI
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
    WeatherAPI,
    LethalLib,
    LethalLevelLoader
  }

  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather", menuName = "LC Weather API/WeatherDefinition", order = 5)]
  public class Weather : ScriptableObject
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType { get; set; } = LevelWeatherType.None;

    [JsonIgnore]
    internal WeatherOrigin Origin { get; set; } = WeatherOrigin.WeatherAPI;

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
    public Color Color { get; set; }

    [field: SerializeField]
    public int DefaultWeight { get; set; } = 50;

    [field: SerializeField]
    public Dictionary<LevelWeatherType, int> WeatherWeights { get; set; } = [];

    [JsonIgnore]
    public AnimationClip AnimationClip;
    public Weather(string name = "None", ImprovedWeatherEffect effect = default)
    {
      Name = name;
      Effect = effect;

      this.name = name;
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
        Color = TerminalStartPatch.VanillaWeatherColors[LevelWeatherType.None],
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

  [CreateAssetMenu(fileName = "WeatherEffect", menuName = "LC Weather API/WeatherEffect", order = 10)]
  public class ImprovedWeatherEffect : ScriptableObject
  {
    [JsonIgnore]
    public GameObject EffectObject;

    [JsonIgnore]
    public GameObject WorldObject;

    private bool _effectEnabled;

    [field: SerializeField]
    public string SunAnimatorBool { get; set; }

    [field: SerializeField]
    public int DefaultVariable1 { get; set; } = 0;

    [field: SerializeField]
    public int DefaultVariable2 { get; set; } = 0;

    public bool EffectEnabled
    {
      get { return _effectEnabled; }
      set
      {
        EffectObject?.SetActive(value);
        WorldObject?.SetActive(value);

        _effectEnabled = value;
      }
    }

    public void DisableEffect(bool permament = false)
    {
      if (permament)
      {
        EffectEnabled = false;
      }
      else
      {
        EffectObject?.SetActive(false);
      }
    }

    public ImprovedWeatherEffect(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;
    }
  }
}