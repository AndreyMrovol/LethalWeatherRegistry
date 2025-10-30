using System;
using UnityEngine;

namespace WeatherRegistry.Definitions
{
  public class IWeatherEffectAbstraction : WeatherEffect
  {
    public ImprovedWeatherEffect Effect { get; internal set; }
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
