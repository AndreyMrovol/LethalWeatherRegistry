using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    internal static Logger Logger = new("WeatherCalculation", LoggingType.Basic);

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
          Plugin.debugLogger.LogInfo("Not a host, cannot generate weathers!");
          return null;
        }

        WeatherCalculation.previousDayWeather.Clear();

        Dictionary<SelectableLevel, LevelWeatherType> NewWeather = [];

        System.Random random = GetRandom(startOfRound);
        List<SelectableLevel> levels = MrovLib.LevelHelper.SortedLevels;

        Logger.LogDebug($"Levels: {string.Join(';', levels.Select(level => ConfigHelper.GetAlphanumericName(level)))}");

        StringBuilder weatherLog = new();

        foreach (SelectableLevel level in levels)
        {
          weatherLog.AppendLine();

          WeatherCalculation.previousDayWeather[level.PlanetName] = level.currentWeather;

          // possible weathers taken from level.randomWeathers (modified by me)
          // use random for seeded randomness

          weatherLog.AppendLine($"Picking weather for {ConfigHelper.GetAlphanumericName(level)} ");

          if (level.overrideWeather)
          {
            weatherLog.AppendLine($"Override weather present, changing weather to {level.overrideWeatherType}");
            Weather overrideWeather = WeatherManager.GetWeather(level.overrideWeatherType);

            NewWeather[level] = overrideWeather.VanillaWeatherType;
            // WeatherManager.CurrentWeathers[level] = overrideWeather;
            EventManager.WeatherChanged.Invoke((level, overrideWeather));

            continue;
          }

          weatherLog.AppendLine($"PreviousDayWeather: {WeatherCalculation.previousDayWeather[level.PlanetName]}");
          weatherLog.AppendLine($"Possible weathers: {string.Join(", ", level.randomWeathers.Select(rw => rw.weatherType))}");

          NewWeather[level] = LevelWeatherType.None;

          WeightHandler<Weather, WeatherWeightType> possibleWeathers = WeatherManager.GetPlanetWeightedList(level);
          Weather selectedWeather = possibleWeathers.Random();

          NewWeather[level] = selectedWeather.VanillaWeatherType;
          EventManager.WeatherChanged.Invoke((level, selectedWeather));

          weatherLog.AppendLine(
            $"Selected weather {selectedWeather.Name} with chance {possibleWeathers.Get(selectedWeather)} / {possibleWeathers.Sum} ({(float)possibleWeathers.Get(selectedWeather) / possibleWeathers.Sum * 100:F2}%)"
          );
        }

        Logger.LogMessage(weatherLog.ToString());
        // Logger.LogMessage("-------------");

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
          num1 = random.Next(15, 25) / 10f;
        }

        int num2 = Mathf.Clamp(
          (int)(
            (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * num1, 0.0f, 1f)
            * startOfRound.levels.Length
          ),
          0,
          startOfRound.levels.Length
        );

        list.ForEach(level =>
        {
          vanillaSelectedWeather[level] = LevelWeatherType.None;

          if (level.overrideWeather)
          {
            Logger.LogDebug($"Override weather present for {level.PlanetName}, changing weather to {level.overrideWeatherType}");
            Weather overrideWeather = WeatherManager.GetWeather(level.overrideWeatherType);

            vanillaSelectedWeather[level] = overrideWeather.VanillaWeatherType;
            // WeatherManager.CurrentWeathers[level] = overrideWeather;
            EventManager.WeatherChanged.Invoke((level, overrideWeather));
          }
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
