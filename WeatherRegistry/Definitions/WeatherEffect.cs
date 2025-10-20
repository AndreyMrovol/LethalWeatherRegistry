using System;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Managers;

namespace WeatherRegistry
{
  [CreateAssetMenu(fileName = "ImprovedEffect", menuName = "WeatherRegistry/ImprovedWeatherEffect", order = 90)]
  public class ImprovedWeatherEffect : ScriptableObject
  {
    [Tooltip("The GameObject that is visible only for the player, i.e. rain particles, sound effects etc.")]
    [SerializeField]
    public GameObject EffectObject;

    [Tooltip("The GameObject that is placed in the world, i.e. floodwater, lightning bolts etc.")]
    [SerializeField]
    public GameObject WorldObject;

    private bool _effectEnabled;

    [Tooltip(
      "The name of sun animator's bool that gets toggled when the weather effect is enabled. Vanilla uses '' for clear weather, 'overcast' for stormy/flooded, 'eclipse' for eclipsed."
    )]
    public string SunAnimatorBool { get; set; }

    [Obsolete]
    public int DefaultVariable1 { get; set; } = 0;

    [Obsolete]
    public int DefaultVariable2 { get; set; } = 0;

    public LevelWeatherType LevelWeatherType { get; internal set; }

    public virtual bool EffectEnabled
    {
      get { return _effectEnabled; }
      set
      {
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

    public bool EffectActive => (EffectObject?.activeSelf).GetValueOrDefault() || (WorldObject?.activeSelf).GetValueOrDefault();

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

      // VanillaWeatherEffect = weatherEffect;

      EffectObject?.SetActive(false);
      WorldObject?.SetActive(false);
    }
  }
}
