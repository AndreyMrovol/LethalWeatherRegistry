using UnityEngine;

namespace WeatherRegistry.Definitions
{
  public class WeatherEffectOverride
  {
    public Weather Weather { get; }
    public SelectableLevel Level { get; }

    public ImprovedWeatherEffect OverrideEffect { get; }

    public WeatherEffectOverride(Weather weather, SelectableLevel level, ImprovedWeatherEffect effect)
    {
      Weather = weather;
      Level = level;
      OverrideEffect = GameObject.Instantiate(effect);

      OverrideEffect.name = $"{ConfigHelper.GetAlphanumericName(Level)}{Weather.VanillaWeatherType}Override";

      WeatherManager.WeatherEffectOverrides.Add(this);
      weather.WeatherEffectOverrides[level] = this;
    }

    public override string ToString()
    {
      return $"{ConfigHelper.GetAlphanumericName(Level)} - {Weather.VanillaWeatherType}";
    }
  }
}
