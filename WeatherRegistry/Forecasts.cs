using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleTables;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Managers;

namespace WeatherRegistry
{
  public static class Forecasts
  {
    private static readonly Logger Logger = new("Forecast", LoggingType.Debug);

    public static (
      List<CompatibleNoun> compatibleNouns,
      List<TerminalNode> forecastNodes,
      List<TerminalKeyword> forecastKeywords
    ) InitializeForecastNodes()
    {
      List<CompatibleNoun> compatibleNouns = [];
      List<TerminalNode> forecastNodes = [];
      List<TerminalKeyword> forecastKeywords = [];

      MrovLib.LevelHelper.Levels.ForEach(level =>
      {
        TerminalNode ForecastNode = new();
        TerminalKeyword ForecastKeyword = new();
        CompatibleNoun ForecastNoun = new();

        string LevelName = ConfigHelper.GetAlphanumericName(level);

        ForecastKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        ForecastKeyword.word = $"{LevelName.ToLowerInvariant()}";
        ForecastKeyword.name = $"Forecast{LevelName}Keyword";
        ForecastKeyword.defaultVerb = Plugin.ForecastVerb;

        ForecastNode = ScriptableObject.CreateInstance<TerminalNode>();
        ForecastNode.name = $"Forecast{LevelName}";
        ForecastNode.clearPreviousText = true;
        ForecastNode.acceptAnything = true;
        ForecastNode.terminalOptions = [];
        ForecastNode.maxCharactersToType = 25;
        ForecastNode.itemCost = 0;
        ForecastNode.buyItemIndex = -1;
        ForecastNode.buyVehicleIndex = -1;
        ForecastNode.buyRerouteToMoon = -1;
        ForecastNode.displayPlanetInfo = -1;
        ForecastNode.shipUnlockableID = -1;
        ForecastNode.creatureFileID = -1;
        ForecastNode.storyLogFileID = -1;
        ForecastNode.playSyncedClip = -1;
        ForecastNode.terminalEvent = "forecast";

        ForecastNode.displayText = GetForecast(level);
        ForecastNoun = new CompatibleNoun() { noun = ForecastKeyword, result = ForecastNode, };
        ForecastKeyword.compatibleNouns = [ForecastNoun];

        compatibleNouns.Add(ForecastNoun);
        forecastNodes.Add(ForecastNode);
        forecastKeywords.Add(ForecastKeyword);

        TerminalNodeManager.ForecastTerminalNodes.Add(ForecastNode, level);
      });

      return (compatibleNouns, forecastNodes, forecastKeywords);
    }

    public static string GetForecast(SelectableLevel level)
    {
      string LevelName = ConfigHelper.GetAlphanumericName(level);
      ConsoleTable outputTable = new("Weather", "Tomorrow");
      StringBuilder outputText = new();

      outputText.AppendLine($"Forecasting weather for {LevelName}");
      outputText.AppendLine($"Current weather: {WeatherManager.GetCurrentWeather(level).Name}\n");
      outputText.AppendLine("Weights for next days:\n");

      Dictionary<string, int> tomorrowWeights = [];

      WeatherManager.GetWeathers().ForEach(weather => tomorrowWeights.Add(weather.Name, weather.GetWeight(level)));
      int totalWeight = tomorrowWeights.Values.Sum();
      tomorrowWeights = tomorrowWeights.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

      Logger.LogDebug($"Total weight for {LevelName}: {totalWeight}");

      foreach (var weather in tomorrowWeights)
      {
        // Logger.LogDebug(
        //   $"Weather: {weather.Key}, Weight: {weather.Value}, Probability: {(float)(weather.Value / (double)totalWeight) * 100}%"
        // );

        outputTable.AddRow(
          // TODO: color
          weather.Key,
          $"{weather.Value.ToString().PadRight(5)} ({((float)(weather.Value / (double)totalWeight) * 100).ToString("0.00")}%)"
        );
      }

      outputText.Append(outputTable.ToStringCustomDecoration(true, false, false));

      return outputText.ToString();
    }
  }
}
