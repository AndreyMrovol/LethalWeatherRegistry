using WeatherRegistry.Modules;

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
  WeatherTweaks,
}

public enum FilteringOption
{
  Include,
  Exclude,
}

public class LevelWeatherVariables
{
  public SelectableLevel Level;

  public int WeatherVariable1;
  public int WeatherVariable2;
}

public class LevelWeather : LevelWeatherVariables
{
  public RegistryWeather Weather;
  public LevelWeatherVariables Variables;
}
