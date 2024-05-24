using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherAPI
{
  public enum WeatherType
  {
    Clear,
    Vanilla,
    Modded,
  }

  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather", menuName = "LC Weather API/WeatherDefinition", order = 5)]
  public class Weather : ScriptableObject
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType = LevelWeatherType.None;

    [JsonProperty]
    public WeatherType Type = WeatherType.Modded;

    [JsonIgnore]
    public Dictionary<SelectableLevel, LevelWeatherVariables> WeatherVariables = [];

    [JsonIgnore]
    public WeatherApiEffect Effect;

    public float ScrapAmountMultiplier = 1;
    public float ScrapValueMultiplier = 1;

    public List<string> LevelBlacklist = [];

    [JsonIgnore]
    public Color Color;
    public int DefaultWeight = 50;

    [JsonIgnore]
    public AnimationClip AnimationClip;

    public Weather(string name, WeatherApiEffect effect)
    {
      Name = name;
      Effect = effect;

      this.name = name;

      // WeatherManager.RegisteredWeathers.Add(this);
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
  public class WeatherApiEffect : ScriptableObject
  {
    [JsonIgnore]
    public GameObject EffectObject;

    [JsonIgnore]
    public GameObject WorldObject;

    private bool _effectEnabled;

    public string SunAnimatorBool;

    public int DefaultVariable1 = 0;
    public int DefaultVariable2 = 0;

    [JsonIgnore]
    public WeatherEffect WeatherEffect;

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

    public WeatherApiEffect(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;
    }
  }
}
