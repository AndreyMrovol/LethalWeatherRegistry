using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherRegistry
{
  public class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];
    internal static SelectableLevel CompanyMoon;

    internal static Dictionary<string, LevelWeatherType> NewWeathers(StartOfRound startOfRound)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        Plugin.logger.LogMessage("Not a host, cannot generate weathers!");
        return null;
      }

      previousDayWeather.Clear();

      Dictionary<string, LevelWeatherType> NewWeather = [];

      int seed = startOfRound.randomMapSeed + 31;
      System.Random random = new System.Random(seed);

      List<LevelWeatherType> VanillaWeatherTypes =
      [
        LevelWeatherType.None,
        LevelWeatherType.DustClouds,
        LevelWeatherType.Rainy,
        LevelWeatherType.Stormy,
        LevelWeatherType.Foggy,
        LevelWeatherType.Flooded,
        LevelWeatherType.Eclipsed,
      ];

      CompanyMoon = StartOfRound.Instance.levels.ToList().Find(level => level.PlanetName == "71 Gordion");

      List<SelectableLevel> levels = startOfRound.levels.ToList();
      int day = startOfRound.gameStats.daysSpent;
      int quota = TimeOfDay.Instance.timesFulfilledQuota;
      int dayInQuota = day % 3;

      foreach (SelectableLevel level in levels)
      {
        previousDayWeather[level.PlanetName] = level.currentWeather;

        if (level.overrideWeather)
        {
          Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");

          NewWeather[level.PlanetName] = level.overrideWeatherType;
          continue;
        }

        // possible weathers taken from level.randomWeathers (modified by me)
        // use random for seeded randomness

        Plugin.logger.LogDebug("-------------");
        Plugin.logger.LogDebug($"{level.PlanetName}");
        Plugin.logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

        NewWeather[level.PlanetName] = LevelWeatherType.None;

        // use weighted lists
        // get all possible random weathers

        Weather previousDayWeatherWeather = WeatherManager.GetWeather(previousDayWeather[level.PlanetName]);
        Dictionary<LevelWeatherType, int> weights = previousDayWeatherWeather.WeatherWeights;

        // get the weighted list of weathers from config
        var weatherWeights = WeatherManager.GetPlanetWeightedList(level, weights);

        if (weatherWeights.Count == 0)
        {
          Plugin.logger.LogWarning($"No possible weathers, setting to None");
          NewWeather[level.PlanetName] = LevelWeatherType.None;
          continue;
        }
        var selectedWeather = weatherWeights[random.Next(0, weatherWeights.Count)];
        Weather weather = WeatherManager.GetWeather(selectedWeather);

        NewWeather[level.PlanetName] = weather.VanillaWeatherType;
        WeatherManager.CurrentWeathers[level] = weather;

        Plugin.logger.LogDebug($"Selected weather: {weather.Name}");
        try
        {
          Plugin.logger.LogDebug(
            $"Chance for that was {weatherWeights.Where(x => x == selectedWeather).Count()} / {weatherWeights.Count} ({(float)weatherWeights.Where(x => x == selectedWeather).Count() / weatherWeights.Count * 100}%)"
          );
        }
        catch { }
      }

      return NewWeather;
    }
  }
}
