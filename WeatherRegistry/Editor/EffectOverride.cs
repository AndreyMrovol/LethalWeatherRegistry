using System;
using UnityEngine;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "WeatherEffectOverride", menuName = "WeatherRegistry/WeatherEffectOverride", order = 6)]
  public class EffectOverride : ScriptableObject
  {
    [Header("Matching properties")]
    public string weatherName;
    public string levelName;

    [Header("Override properties")]
    public ImprovedWeatherEffect OverrideEffect;

    [Header("Optional: Display properties")]
    public string weatherDisplayName;
    public Color weatherDisplayColor;
  }
}
