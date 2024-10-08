using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace WeatherRegistry
{
  public static class WeatherController
  {
    private static ManualLogSource Logger = new("WeatherController");

    #region Change weather
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
      if (!Settings.SelectWeathers)
      {
        return;
      }

      WeatherManager.currentWeathers.SetWeather(level, weather);
      level.currentWeather = weather.VanillaWeatherType;

      Logger.LogDebug($"Changed weather for {ConfigHelper.GetNumberlessName(level)} to {weather.VanillaWeatherType}");

      EventManager.WeatherChanged.Invoke((level, weather));
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    #endregion

    #region Change random weathers

    public static void SetRandomWeathers(SelectableLevel level, List<RandomWeatherWithVariables> randomWeathers)
    {
      SetRandomWeathers(level, randomWeathers.ToArray());
    }

    public static void SetRandomWeathers(SelectableLevel level, RandomWeatherWithVariables[] randomWeathers)
    {
      level.randomWeathers = randomWeathers.ToArray();

      string levelName = ConfigHelper.GetAlphanumericName(level);

      foreach (RandomWeatherWithVariables randomWeather in randomWeathers)
      {
        Logger.LogWarning($"Adding random weather {randomWeather.weatherType} to {levelName}");

        WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables[level] = new()
        {
          Level = level,
          WeatherVariable1 = randomWeather.weatherVariable,
          WeatherVariable2 = randomWeather.weatherVariable2
        };
      }
    }

    public static void AddRandomWeather(SelectableLevel level, RandomWeatherWithVariables randomWeather)
    {
      List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();
      randomWeathers.Add(randomWeather);
      level.randomWeathers = randomWeathers.ToArray();

      WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables[level] = new()
      {
        Level = level,
        WeatherVariable1 = randomWeather.weatherVariable,
        WeatherVariable2 = randomWeather.weatherVariable2
      };
    }

    public static void RemoveRandomWeather(SelectableLevel level, RandomWeatherWithVariables randomWeather)
    {
      List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();
      randomWeathers.Remove(randomWeather);
      level.randomWeathers = randomWeathers.ToArray();

      WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables.Remove(level);
    }

    #endregion
  }
}
