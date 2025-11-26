using System.Collections.Generic;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Managers
{
  /// <summary>
  /// Manager responsible for handling weather effects as well as planet name overrides.
  /// </summary>
  public static class OverridesManager
  {
    public static List<WeatherEffectOverride> WeatherEffectOverrides { get; internal set; } = [];
    public static Dictionary<WeatherEffectOverride, string> PlanetOverrideNames { get; internal set; } = [];

    public static WeatherEffectOverride GetCurrentWeatherOverride(SelectableLevel level, Weather weather)
    {
      weather.WeatherEffectOverrides.TryGetValue(level, out WeatherEffectOverride weatherEffectOverride);

      return weatherEffectOverride;
    }

    public static string GetPlanetOverrideName(WeatherEffectOverride overrideEffect)
    {
      if (PlanetOverrideNames.TryGetValue(overrideEffect, out string planetName))
      {
        return planetName;
      }

      return null;
    }
  }
}
