using UnityEngine;

namespace WeatherRegistry.Definitions
{
  public class WeatherEffectOverride
  {
    public Weather Weather { get; }
    public SelectableLevel Level { get; }

    public ImprovedWeatherEffect OverrideEffect { get; }

    public string DisplayName { get; set; }
    public Color DisplayColor { get; set; }

    public WeatherEffectOverride(Weather weather, SelectableLevel level, ImprovedWeatherEffect effect)
      : this(weather, level, effect, null, default) { }

    public WeatherEffectOverride(Weather weather, SelectableLevel level, ImprovedWeatherEffect effect, string displayName, Color displayColor)
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

      Plugin.logger.LogWarning($"this.DisplayName: {DisplayName}");
    }

    public override string ToString()
    {
      return $"{ConfigHelper.GetAlphanumericName(Level)} - {Weather.VanillaWeatherType} {(!string.IsNullOrEmpty(DisplayName) ? $"({DisplayName})" : "")}";
    }
  }
}
