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

    public bool EffectActive => EffectObject?.activeSelf ?? false;

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

    public ImprovedWeatherEffect(WeatherEffect weatherEffect)
    {
      EffectObject = weatherEffect.effectObject;
      WorldObject = weatherEffect.effectPermanentObject;
    }
  }
}
