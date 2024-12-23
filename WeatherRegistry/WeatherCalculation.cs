using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    public static MrovLib.Logger Logger = new("WeatherRegistry", ConfigManager.LogWeatherChanges);

    internal static WeatherSelectionAlgorithm RegistryAlgorithm = new WeatherRegistryWeatherSelection();
    internal static WeatherSelectionAlgorithm VanillaAlgorithm = new VanillaWeatherSelection();

    [Obsolete("Use Settings.WeatherSelectionAlgorithm instead")]
    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm
    {
      get { return Settings.WeatherSelectionAlgorithm; }
      set { Settings.WeatherSelectionAlgorithm = value; }
    }

    internal class WeatherRegistryWeatherSelection : WeatherSelectionAlgorithm
    {
      public override Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
      {
        if (!startOfRound.IsHost)
        {
          Logger.LogInfo("Not a host, cannot generate weathers!");
          return null;
        }

        WeatherCalculation.previousDayWeather.Clear();

        Dictionary<SelectableLevel, LevelWeatherType> NewWeather = [];

        System.Random random = GetRandom(startOfRound);

        List<SelectableLevel> levels = startOfRound.levels.ToList();
        // int day = startOfRound.gameStats.daysSpent;
        // int quota = TimeOfDay.Instance.timesFulfilledQuota;
        // int dayInQuota = day % 3;

        Logger.LogDebug($"Levels: {string.Join(';', levels.Select(level => level.PlanetName))}");

        foreach (SelectableLevel level in levels)
        {
          WeatherCalculation.previousDayWeather[level.PlanetName] = level.currentWeather;

          // possible weathers taken from level.randomWeathers (modified by me)
          // use random for seeded randomness

          Logger.LogMessage("-------------");
          Logger.LogMessage($"{level.PlanetName}");
          Logger.LogDebug($"previousDayWeather: {WeatherCalculation.previousDayWeather[level.PlanetName]}");

          if (level.overrideWeather)
          {
            Logger.LogMessage($"Override weather present, changing weather to {level.overrideWeatherType}");
            Weather overrideWeather = WeatherManager.GetWeather(level.overrideWeatherType);

            NewWeather[level] = overrideWeather.VanillaWeatherType;
            // WeatherManager.CurrentWeathers[level] = overrideWeather;
            EventManager.WeatherChanged.Invoke((level, overrideWeather));

            continue;
          }

          NewWeather[level] = LevelWeatherType.None;

          MrovLib.WeightHandler<Weather> possibleWeathers = WeatherManager.GetPlanetWeightedList(level);
          Weather selectedWeather = possibleWeathers.Random();

          NewWeather[level] = selectedWeather.VanillaWeatherType;
          // WeatherManager.CurrentWeathers[level] = selectedWeather;
          EventManager.WeatherChanged.Invoke((level, selectedWeather));

          Logger.LogMessage($"Selected weather: {selectedWeather.Name}");
          Logger.LogMessage(
            $"Chance for that was {possibleWeathers.Get(selectedWeather)} / {possibleWeathers.Sum} ({(float)possibleWeathers.Get(selectedWeather) / possibleWeathers.Sum * 100}%)"
          );
        }

        Logger.LogMessage("-------------");

        return NewWeather;
      }
    }

    internal class VanillaWeatherSelection : WeatherSelectionAlgorithm
    {
      public override Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
      {
        Dictionary<SelectableLevel, LevelWeatherType> vanillaSelectedWeather = [];

        // vanilla algorithm tweaked to work within Registry

        System.Random random = GetRandom(startOfRound);
        List<SelectableLevel> list = startOfRound.levels.ToList();
        float num1 = 1f;

        if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
        {
          num1 = (float)random.Next(15, 25) / 10f;
        }

        int num2 = Mathf.Clamp(
          (int)(
            (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * num1, 0.0f, 1f)
            * (double)startOfRound.levels.Length
          ),
          0,
          startOfRound.levels.Length
        );

        list.ForEach(level =>
        {
          vanillaSelectedWeather[level] = LevelWeatherType.None;
        });

        Logger.LogMessage("Selected vanilla algorithm - weights are not being used!");
        Logger.LogMessage($"Picking weathers for {num2} moons:");
        Logger.LogMessage("-------------");

        for (int index = 0; index < num2; ++index)
        {
          SelectableLevel selectableLevel = list[random.Next(0, list.Count)];

          if (selectableLevel.randomWeathers != null && selectableLevel.randomWeathers.Length != 0)
          {
            vanillaSelectedWeather[selectableLevel] = selectableLevel
              .randomWeathers[random.Next(0, selectableLevel.randomWeathers.Length)]
              .weatherType;

            Logger.LogMessage($"Selected weather for {selectableLevel.PlanetName}: {vanillaSelectedWeather[selectableLevel].ToString()}");
          }
          else
          {
            Logger.LogDebug($"Cannot pick weather for {selectableLevel.PlanetName}");
          }
          list.Remove(selectableLevel);
        }

        Logger.LogMessage("-------------");

        return vanillaSelectedWeather;
      }
    }
  }
}
