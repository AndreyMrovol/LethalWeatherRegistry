using System;
using UnityEngine;
using UnityEngine.Events;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "WeatherEffectOverride", menuName = "WeatherRegistry/Overrides/WeatherEffectOverride", order = 3)]
  public class EffectOverride : ScriptableObject
  {
    [Header("Matching properties")]
    [Tooltip("Weather name to match the override to")]
    public string weatherName;

    [Tooltip("Semicolon-separated list of level names to match the override to")]
    public string levelName;

    [Header("Override properties")]
    public ImprovedWeatherEffect OverrideEffect;

    [Header("Optional: Display properties")]
    [Tooltip("Display name for the weather effect override")]
    public string weatherDisplayName;

    [Tooltip("Display color for the weather effect override")]
    public Color weatherDisplayColor;
  }
}
