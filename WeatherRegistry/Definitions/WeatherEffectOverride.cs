using WeatherRegistry.Modules;

namespace WeatherRegistry.Definitions
{
  public class WeatherEffectOverride
  {
    public RegistryWeather Weather { get; }
    public SelectableLevel Level { get; }

    public RegistryWeatherEffect OverrideEffect { get; }

    public WeatherEffectOverride(RegistryWeather weather, SelectableLevel level, RegistryWeatherEffect effect)
    {
      Weather = weather;
      Level = level;
      OverrideEffect = effect;

      WeatherManager.WeatherEffectOverrides.Add(this);
      weather.WeatherEffectOverrides[level] = this;
    }
  }
}
