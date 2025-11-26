using System;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Managers;

namespace WeatherRegistry
{
  [Obsolete("Use WeatherRegistry.Definitions.ImprovedWeatherEffect instead")]
  public class ImprovedWeatherEffect : Definitions.ImprovedWeatherEffect
  {
    public ImprovedWeatherEffect(GameObject effectObject, GameObject worldObject)
      : base(effectObject, worldObject) { }

    public ImprovedWeatherEffect(WeatherEffect weatherEffect)
      : base(weatherEffect) { }
  }
}
