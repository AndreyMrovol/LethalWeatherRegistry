using System;

namespace WeatherRegistry.Enums
{
  public enum WeatherType
  {
    Clear,
    Vanilla,
    Modded,
  }

  public enum WeatherOrigin
  {
    Vanilla,
    WeatherRegistry,
    LethalLib,
    LethalLevelLoader,
    DawnLib,
    JacobLib,
    WeatherTweaks,
  }

  public enum FilteringOption
  {
    Include,
    Exclude,
  }

  public enum WeatherWeightType
  {
    Default,
    WeatherToWeather,
    Level,
  }

  public class LevelWeatherVariables
  {
    public SelectableLevel Level;

    public int WeatherVariable1;
    public int WeatherVariable2;
  }
}
