using System.Collections.Generic;
using System.Linq;
using MrovLib;

namespace WeatherRegistry
{
  public static class WeatherController
  {
    private static Logger Logger = new("WeatherController", LoggingType.Basic);

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
      // if something else is controlling the weather sync, don't change it
      if (!Settings.SelectWeathers)
      {
        return;
      }

      if (weather == null)
      {
        Logger.LogWarning("Weather is null, cannot change weather");
        return;
      }

      level.currentWeather = weather.VanillaWeatherType;

      if (StartOfRound.Instance.currentLevel == level)
      {
        TimeOfDay.Instance.currentLevelWeather = weather.VanillaWeatherType;
      }

      Logger.LogDebug($"Changed weather for {ConfigHelper.GetNumberlessName(level)} to {weather.VanillaWeatherType}");

      // if ship has already landed, don't change the weathers
      // only change the current effects
      if (!StartOfRound.Instance.inShipPhase)
      {
        if (StartOfRound.Instance.currentLevel == level)
        {
          Logger.LogDebug("Ship has already landed, changing weather effects");
          SetWeatherEffects(weather);
          return;
        }
        else
        {
          Logger.LogDebug("Ship has already landed - cannot change weather effect on other level!");
        }
      }

      WeatherManager.CurrentWeathers.SetWeather(level, weather);

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

        // WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables[level] = new()
        // {
        //   Level = level,
        //   WeatherVariable1 = randomWeather.weatherVariable,
        //   WeatherVariable2 = randomWeather.weatherVariable2
        // };
      }
    }

    public static void AddRandomWeather(SelectableLevel level, RandomWeatherWithVariables randomWeather)
    {
      List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();

      if (randomWeather.weatherType == LevelWeatherType.None)
      {
        Logger.LogInfo("Cannot add None weather to random weathers, skipping");
        return;
      }

      randomWeathers.Add(randomWeather);
      level.randomWeathers = randomWeathers.Distinct().ToArray();

      Plugin.logger.LogInfo($"Adding random weather {randomWeather.weatherType} to {ConfigHelper.GetAlphanumericName(level)}");

      // WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables[level] = new()
      // {
      //   Level = level,
      //   WeatherVariable1 = randomWeather.weatherVariable,
      //   WeatherVariable2 = randomWeather.weatherVariable2
      // };
    }

    public static void RemoveRandomWeather(SelectableLevel level, RandomWeatherWithVariables randomWeather)
    {
      List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();
      randomWeathers.Remove(randomWeather);
      level.randomWeathers = randomWeathers.ToArray();

      // WeatherManager.GetWeather(randomWeather.weatherType).WeatherVariables.Remove(level);
    }

    public static void RemoveRandomWeather(SelectableLevel level, LevelWeatherType weatherType)
    {
      List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.Where(rw => rw.weatherType == weatherType).ToList();

      foreach (RandomWeatherWithVariables randomWeather in randomWeathers)
      {
        RemoveRandomWeather(level, randomWeather);
      }
    }

    #endregion

    #region Weather effects

    public static void SetWeatherEffects(LevelWeatherType weatherType)
    {
      WeatherSync.Instance.SetWeatherEffectOnHost(weatherType);
    }

    public static void SetWeatherEffects(Weather weather)
    {
      SetWeatherEffects(weather.VanillaWeatherType);
    }

    public static void AddWeatherEffect(LevelWeatherType weatherType)
    {
      Plugin.logger.LogDebug($"Adding weather effect {weatherType}");

      List<LevelWeatherType> effects = WeatherSync.Instance.Effects.Effects.ToList();
      effects.Add(weatherType);

      WeatherSync.Instance.SetWeatherEffectsOnHost(effects.ToArray());
    }

    public static void AddWeatherEffect(Weather weather)
    {
      AddWeatherEffect(weather.VanillaWeatherType);
    }

    #endregion
  }
}
