using System;
using TMPro;
using UnityEngine;

namespace WeatherRegistry.Definitions
{
  public class WeatherEffectOverride
  {
    public ImprovedWeather Weather { get; }
    public SelectableLevel Level { get; }

    public ImprovedWeatherEffect OverrideEffect { get; }

    public string DisplayName { get; set; }
    public Color DisplayColor { get; set; }

    public float Chance { get; set; } = 1f;

    [Obsolete("Use the constructor that includes displayName and displayColor parameters instead.")]
    public WeatherEffectOverride(ImprovedWeather weather, SelectableLevel level, ImprovedWeatherEffect effect)
      : this(weather, level, effect, null, default) { }

    public WeatherEffectOverride(
      ImprovedWeather weather,
      SelectableLevel level,
      ImprovedWeatherEffect effect,
      string displayName,
      Color displayColor,
      float chance = 1f
    )
    {
      Weather = weather;
      Level = level;

      DisplayName = displayName;
      DisplayColor = displayColor;

      OverrideEffect = GameObject.Instantiate(effect);
      OverrideEffect.name = $"{ConfigHelper.GetAlphanumericName(Level)}{Weather.VanillaWeatherType}Override";

      if (!string.IsNullOrEmpty(DisplayName) && DisplayColor != default)
      {
        if (!Settings.ScreenMapColors.ContainsKey(DisplayName))
        {
          Settings.ScreenMapColors.Add(DisplayName, DisplayColor);
        }
      }

      // Add to collections after everything is initialized
      WeatherOverrideManager.WeatherEffectOverrides.Add(this);
      weather.WeatherEffectOverrides[level] = this;
    }

    public override string ToString()
    {
      return $"{ConfigHelper.GetAlphanumericName(Level)} - {Weather.VanillaWeatherType} {(!string.IsNullOrEmpty(DisplayName) ? $"({DisplayName})" : "")}";
    }
  }
}
