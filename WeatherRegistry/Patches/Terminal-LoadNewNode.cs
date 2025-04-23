using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
  class TerminalLoadNewNodePatch
  {
    [HarmonyPostfix]
    public static void GameMethodPatch(Terminal __instance, ref TerminalNode node)
    {
      if (WeatherManager.ForecastTerminalNodes.Keys.Contains(node))
      {
        Weather weather = WeatherManager.ForecastTerminalNodes[node];
        Plugin.debugLogger.LogWarning($"Forecasting weather {weather.name}");

        var w2wtable = new ConsoleTables.ConsoleTable(
          "Weather", // Name
          "Weight",
          "% chance"
        );

        var adjustedTable = new StringBuilder();

        adjustedTable.Append($"\n\nForecasting weather {weather.name}:\n\n");
        adjustedTable.Append($"Default weight: {weather.DefaultWeight}\n\n");

        int totalWeightPool = 0;

        Dictionary<Weather, int> weights = [];
        foreach (var weather2 in WeatherManager.Weathers)
        {
          (bool isWTW, int weight) = weather.GetWeatherToWeatherWeight(weather2);

          if (!isWTW)
          {
            continue;
          }

          totalWeightPool += weight;
          weights.Add(weather2, weight);
        }

        weights = weights.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (KeyValuePair<Weather, int> kvp in weights)
        {
          w2wtable.AddRow(kvp.Key.name.PadRight(20), kvp.Value, (kvp.Value / (float)totalWeightPool * 100).ToString("0.00") + "%");
        }

        w2wtable.AddRow("", "", "");
        w2wtable.AddRow("", "", "");
        w2wtable.AddRow("", totalWeightPool.ToString().PadRight(6), "100%".ToString().PadLeft(4));

        adjustedTable.Append("Weather-to-weather weights:\n");
        adjustedTable.Append(w2wtable.ToStringCustomDecoration(header: true));

        adjustedTable.Append("----\n\n");
        adjustedTable.Append("Level weights:\n");

        var levelsTable = new ConsoleTables.ConsoleTable("Level", "Weight");
        foreach (SelectableLevel level in weather.LevelWeights.Keys)
        {
          levelsTable.AddRow(level.name, weather.LevelWeights[level]);
        }

        adjustedTable.Append(levelsTable.ToStringCustomDecoration(header: true));

        __instance.currentText = adjustedTable.ToString();
      }
    }
  }
}
