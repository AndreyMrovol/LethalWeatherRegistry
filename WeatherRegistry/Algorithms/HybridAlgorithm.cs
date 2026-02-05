using System.Collections.Generic;
using System.Linq;
using System.Text;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Algorithms;

internal class HybridWeatherSelection : WeatherSelectionAlgorithm
{
  Dictionary<SelectableLevel, LevelWeatherType> SelectedWeathers = [];

  public override Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
  {
    if (!startOfRound.IsHost)
    {
      Plugin.debugLogger.LogInfo("Not a host, cannot generate weathers!");
      return null;
    }

    System.Random random = GetRandom(startOfRound);

    // use vanilla algorithm for deciding how many moons should have weather
    float num1 = 1f;

    if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
    {
      num1 = random.Next(15, 35) / 5f;
    }

    int num2 = Mathf.Clamp(
      (int)(
        (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * num1, 0.0f, 1f)
        * startOfRound.levels.Length
      ),
      0,
      startOfRound.levels.Length
    );

    List<SelectableLevel> levels = LevelHelper.SortedLevels.ToList();
    int longestPlanetName = LevelHelper.LongestPlanetName.Length;
    bool isDebugLoggingEnabled = ConfigManager.LoggingLevels.Value >= LoggingType.Debug;

    levels.ForEach(level =>
    {
      SelectedWeathers[level] = LevelWeatherType.None;

      if (level.overrideWeather)
      {
        Logger.LogDebug($"Override weather present for {level.PlanetName}, changing weather to {level.overrideWeatherType}");
        Weather overrideWeather = WeatherManager.GetWeather(level.overrideWeatherType);

        SelectedWeathers[level] = overrideWeather.VanillaWeatherType;
        EventManager.WeatherChanged.Invoke((level, overrideWeather));
      }
    });

    StringBuilder weatherLog = new();
    weatherLog.AppendLine($"Picking weathers for {num2}/{levels.Count} moons:");

    for (int index = 0; index < num2; ++index)
    {
      weatherLog.AppendLine();
      SelectableLevel selectableLevel = levels[random.Next(0, levels.Count)];
      WeatherCalculation.previousDayWeather[selectableLevel.PlanetName] = selectableLevel.currentWeather;

      if (!isDebugLoggingEnabled)
      {
        weatherLog.Append('\t');
      }
      weatherLog.Append($"[{ConfigHelper.GetAlphanumericName(selectableLevel)}] ".PadRight(isDebugLoggingEnabled ? 0 : longestPlanetName + 4));

      if (selectableLevel.randomWeathers != null && selectableLevel.randomWeathers.Length != 0)
      {
        Utils.WeightHandler<Weather, WeatherWeightType> possibleWeathers = WeatherManager.GetPlanetWeightedList(selectableLevel);
        Weather selectedWeather = possibleWeathers.Random();

        SelectedWeathers[selectableLevel] = selectedWeather.VanillaWeatherType;
        weatherLog.Append(
          $"Selected weather: {selectedWeather.Name} ({(float)possibleWeathers.Get(selectedWeather) / possibleWeathers.Sum * 100:F2}% chance)"
        );
        EventManager.WeatherChanged.Invoke((selectableLevel, selectedWeather));

        if (isDebugLoggingEnabled)
        {
          weatherLog.AppendLine();

          weatherLog.AppendLine($"PreviousDayWeather: {WeatherCalculation.previousDayWeather[selectableLevel.PlanetName]}");
          weatherLog.AppendLine($"Possible weathers: [{string.Join(", ", selectableLevel.randomWeathers.Select(rw => rw.weatherType))}]");
          weatherLog.AppendLine($"Used weights: ");
          foreach (Weather weather in possibleWeathers.Keys)
          {
            weatherLog.AppendLine(
              $"  {weather.Name} has {possibleWeathers.GetOrigin(weather).ToString().ToLowerInvariant()} weight ({possibleWeathers.Get(weather)})"
            );
          }
        }
      }
      else
      {
        weatherLog.AppendLine($"Cannot pick weather for {selectableLevel.PlanetName}");
      }
      levels.Remove(selectableLevel);
    }

    Logger.LogInfo(weatherLog.ToString());

    return SelectedWeathers;
  }
}
