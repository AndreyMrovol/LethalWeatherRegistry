using System;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Managers;

namespace WeatherRegistry
{
  [CreateAssetMenu(fileName = "WeatherEffect", menuName = "WeatherRegistry/ImprovedWeatherEffect", order = 90)]
  public class ImprovedWeatherEffect : ScriptableObject
  {
    [JsonIgnore]
    [Tooltip("The GameObject that is visible only for the player, i.e. rain particles, sound effects etc.")]
    [SerializeField]
    public GameObject EffectObject;

    [JsonIgnore]
    [Tooltip("The GameObject that is placed in the world, i.e. floodwater, lightning bolts etc.")]
    [SerializeField]
    public GameObject WorldObject;

    private bool _effectEnabled;

    [field: SerializeField]
    [Tooltip(
      "The name of sun animator's bool that gets toggled when the weather effect is enabled. Vanilla uses '' for clear weather, 'overcast' for stormy/flooded, 'eclipse' for eclipsed."
    )]
    public string SunAnimatorBool { get; set; }

    [field: SerializeField]
    public int DefaultVariable1 { get; set; } = 0;

    [field: SerializeField]
    public int DefaultVariable2 { get; set; } = 0;

    public LevelWeatherType LevelWeatherType { get; internal set; }

    //  <summary>
    ////  Use TimeOfDay.Instance.effects[LevelWeatherType] wherever possible.
    //  </summary>
    public WeatherEffect VanillaWeatherEffect { get; internal set; }

    public virtual bool EffectEnabled
    {
      get { return _effectEnabled; }
      set
      {
        // i wish i was joking while writing this abysmal dogshit check but NO, it actually happened
        if (this == null || !this)
        {
          WeatherEffectManager.Logger.LogDebug("Attempted to set EffectEnabled on a destroyed ImprovedWeatherEffect, skipping!");
          return;
        }

        WeatherEffectManager.Logger.LogDebug($"Setting effect {this.name} to {value} - is player inside? {Settings.IsPlayerInside}");

        if (!Settings.IsPlayerInside)
        {
          EffectObject?.SetActive(value);
          WeatherEffectController.SetTimeOfDayEffect(LevelWeatherType, value);
        }

        WorldObject?.SetActive(value);

        _effectEnabled = value;
      }
    }

    public bool EffectActive => EffectObject?.activeSelf ?? false;

    public virtual void DisableEffect(bool permament = false)
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

    [Obsolete("Use Utils.InstanceCreator<ImprovedWeatherEffect> instead!")]
    public ImprovedWeatherEffect(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;

      EffectObject?.SetActive(false);
      WorldObject?.SetActive(false);
    }

    public ImprovedWeatherEffect(WeatherEffect weatherEffect)
    {
      EffectObject = weatherEffect.effectObject;
      WorldObject = weatherEffect.effectPermanentObject;

      VanillaWeatherEffect = weatherEffect;

      EffectObject?.SetActive(false);
      WorldObject?.SetActive(false);
    }
  }
}
