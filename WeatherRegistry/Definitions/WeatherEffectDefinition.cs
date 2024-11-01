using System;
using UnityEngine;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Definitions
{
  public interface IWeatherEffectDefinition
  {
    /// <value>An uninstantiated EffectObject prefab</value>
    GameObject EffectObject { get; }

    /// <value>An uninstantiated WorldObject prefab</value>
    GameObject WorldObject { get; }

    /// <summary>SunAnimator bool</summary>
    /// <value>"", "overcast", "eclipsed"</value>
    string SunAnimatorBool { get; set; }
    int DefaultVariable1 { get; set; }
    int DefaultVariable2 { get; set; }
  }

  public class WeatherEffectDefinition : IWeatherEffectDefinition
  {
    public GameObject EffectObject { get; set; } = null;
    public GameObject WorldObject { get; set; } = null;
    public string SunAnimatorBool { get; set; } = "";
    public int DefaultVariable1 { get; set; } = 1;
    public int DefaultVariable2 { get; set; } = 1;

    public WeatherEffectDefinition(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;
    }

    [Obsolete]
    public WeatherEffectDefinition(ImprovedWeatherEffect improvedWeatherEffect)
    {
      EffectObject = improvedWeatherEffect.EffectObject;
      WorldObject = improvedWeatherEffect.WorldObject;
      SunAnimatorBool = improvedWeatherEffect.SunAnimatorBool;
      DefaultVariable1 = improvedWeatherEffect.DefaultVariable1;
      DefaultVariable2 = improvedWeatherEffect.DefaultVariable2;
    }

    public WeatherEffectDefinition(WeatherEffect weatherEffect)
    {
      EffectObject = weatherEffect.effectObject;
      WorldObject = weatherEffect.effectPermanentObject;
      SunAnimatorBool = weatherEffect.sunAnimatorBool;
    }
  }
}
