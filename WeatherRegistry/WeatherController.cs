using System.Collections.Generic;

namespace WeatherRegistry
{
  public class WeatherController
  {
    public static void ChangeCurrentWeather(Weather weather)
    {
      SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;

      ChangeWeather(currentLevel, weather);
    }

    public static void ChangeCurrentWeather(LevelWeatherType weatherType)
    {
      SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;

      ChangeWeather(currentLevel, weatherType);
    }

    public static void ChangeWeather(SelectableLevel level, LevelWeatherType weatherType)
    {
      Weather weather = WeatherManager.GetWeather(weatherType);

      ChangeWeather(level, weather);
    }

    // this is the one that every overload should resolve to
    public static void ChangeWeather(SelectableLevel level, Weather weather)
    {
      WeatherManager.CurrentWeathers[level] = weather;
      level.currentWeather = weather.VanillaWeatherType;

      EventManager.WeatherChanged.Invoke((level, weather));
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
