using System;
using UnityEngine;

namespace WeatherRegistry
{
  public class IWeatherEffectAbstraction : WeatherEffect
  {
    public ImprovedWeatherEffect Effect;

    #region Properties from vanilla WeatherEffect

    // public new GameObject effectObject => Effect?.EffectObject;
    // public GameObject effectPermanentObject => Effect.WorldObject;
    // public bool effectEnabled => Effect.EffectEnabled;
    // public string sunAnimatorBool => Effect.SunAnimatorBool;

    #endregion
  }

  public interface IWeatherEffect
  {
    string name { get; }

    GameObject effectObject { get; }
    GameObject effectPermanentObject { get; }
    bool effectEnabled { get; }

    string sunAnimatorBool { get; }
  }

  internal interface IWeatherEffectAdditionalProperties
  {
    bool lerpPosition { get; }
    bool transitioning { get; }
  }
}
