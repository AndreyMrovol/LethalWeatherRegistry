using Newtonsoft.Json;
using UnityEngine;

namespace WeatherRegistry
{
  [CreateAssetMenu(fileName = "WeatherEffect", menuName = "WeatherRegistry/WeatherEffect", order = 10)]
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

    public LevelWeatherType LevelWeatherType { get; set; } = LevelWeatherType.None;

    public virtual bool EffectEnabled
    {
      get { return _effectEnabled; }
      set
      {
        Plugin.logger.LogDebug($"Setting effect {this.name} to {value}");
        Plugin.logger.LogDebug($"Is player inside? {Settings.IsPlayerInside}");

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

      EffectObject?.SetActive(false);
      WorldObject?.SetActive(false);
    }
  }
}
