using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Algorithms;

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
