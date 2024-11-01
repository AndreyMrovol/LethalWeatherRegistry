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
  public class RegistryWeatherEffect : ScriptableObject, IWeatherEffectDefinition
  {
    [JsonIgnore]
    public GameObject EffectObject { get; set; }

    [JsonIgnore]
    public GameObject WorldObject { get; set; }

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
        Plugin.logger.LogDebug($"Setting effect {this.name} to {value}");

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

    public RegistryWeatherEffect(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;
    }

    public RegistryWeatherEffect(WeatherEffect weatherEffect)
    {
      EffectObject = weatherEffect.effectObject;
      WorldObject = weatherEffect.effectPermanentObject;
      SunAnimatorBool = weatherEffect.sunAnimatorBool;
    }

    public RegistryWeatherEffect(WeatherEffectDefinition definition)
    {
      definition ??= default;

      EffectObject = definition.EffectObject;
      WorldObject = definition.WorldObject;
      SunAnimatorBool = definition.SunAnimatorBool;
      DefaultVariable1 = definition.DefaultVariable1;
      DefaultVariable2 = definition.DefaultVariable2;

      if (EffectObject != null)
      {
        GameObject.Instantiate(EffectObject);
        EffectObject.hideFlags = HideFlags.HideAndDontSave;
        GameObject.DontDestroyOnLoad(EffectObject);
      }

      if (WorldObject != null)
      {
        GameObject.Instantiate(WorldObject);
        WorldObject.hideFlags = HideFlags.HideAndDontSave;
        GameObject.DontDestroyOnLoad(WorldObject);
      }
    }
  }
}
