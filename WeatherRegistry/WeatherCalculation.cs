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
      System.Random random = new(seed);

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

        Plugin.logger.LogMessage("-------------");
        Plugin.logger.LogMessage($"{level.PlanetName}");
        Plugin.logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

        NewWeather[level.PlanetName] = LevelWeatherType.None;

        // get the weighted list of weathers
        var weatherWeights = WeatherManager.GetPlanetWeightedList(level);

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

        Plugin.logger.LogMessage($"Selected weather: {weather.Name}");
        try
        {
          Plugin.logger.LogMessage(
            $"Chance for that was {weatherWeights.Where(x => x == selectedWeather).Count()} / {weatherWeights.Count} ({(float)weatherWeights.Where(x => x == selectedWeather).Count() / weatherWeights.Count * 100}%)"
          );
        }
        catch { }
      }
      Plugin.logger.LogMessage("-------------");

      return NewWeather;
    }
  }
}
