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
      OverrideEffect = effect;

      WeatherManager.WeatherEffectOverrides.Add(this);
      weather.WeatherEffectOverrides[level] = this;
    }
  }
}
