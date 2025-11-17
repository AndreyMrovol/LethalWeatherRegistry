using System.Collections.Generic;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Managers
{
  public static class WeatherOverrideManager
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
