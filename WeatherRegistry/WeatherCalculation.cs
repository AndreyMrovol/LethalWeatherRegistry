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

    public static MrovLib.Logger Logger = new("WeatherRegistry", ConfigManager.LogWeatherChanges);

    internal static Dictionary<string, LevelWeatherType> NewWeathers(StartOfRound startOfRound)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        Logger.LogInfo("Not a host, cannot generate weathers!");
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
          Logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");

          NewWeather[level.PlanetName] = level.overrideWeatherType;
          continue;
        }

        // possible weathers taken from level.randomWeathers (modified by me)
        // use random for seeded randomness

        Logger.LogMessage("-------------");
        Logger.LogMessage($"{level.PlanetName}");
        Logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

        NewWeather[level.PlanetName] = LevelWeatherType.None;

        MrovLib.WeightHandler<Weather> possibleWeathers = WeatherManager.GetPlanetWeightedList(level);
        Weather selectedWeather = possibleWeathers.Random();

        NewWeather[level.PlanetName] = selectedWeather.VanillaWeatherType;
        WeatherManager.CurrentWeathers[level] = selectedWeather;

        EventManager.WeatherChanged.Invoke((level, selectedWeather));

        Logger.LogMessage($"Selected weather: {selectedWeather.Name}");
        try
        {
          Logger.LogMessage(
            $"Chance for that was {possibleWeathers.Get(selectedWeather)} / {possibleWeathers.Sum} ({(float)possibleWeathers.Get(selectedWeather) / possibleWeathers.Sum * 100}%)"
          );
        }
        catch { }
      }
      Logger.LogMessage("-------------");

      return NewWeather;
    }
  }
}
