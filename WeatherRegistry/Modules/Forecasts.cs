using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleTables;
using LobbyCompatibility.Configuration;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Enums;
using WeatherRegistry.Helpers;
using WeatherRegistry.Managers;

namespace WeatherRegistry.Modules
{
  public static class Forecasts
  {
    private static readonly Logger Logger = new("Forecast", LoggingType.Debug);

    public static void InitializeForecastNodes(TerminalKeyword ForecastVerb)
    {
      List<CompatibleNoun> compatibleNouns = [];
      List<TerminalNode> forecastNodes = [];
      List<TerminalKeyword> forecastKeywords = [];

      MrovLib.LevelHelper.Levels.ForEach(level =>
      {
        string LevelName = ConfigHelper.GetAlphanumericName(level);

        TerminalNode ForecastNode = TerminalNodeManager.CreateTerminalNode($"Forecast{level.PlanetName}", "forecast");
        ForecastNode.displayText = GetForecast(level);

        TerminalKeyword ForecastKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        ForecastKeyword.word = $"{LevelName.ToLowerInvariant()}";
        ForecastKeyword.name = $"Forecast{LevelName}Keyword";
        ForecastKeyword.defaultVerb = ForecastVerb;

        CompatibleNoun ForecastNoun = new();

        ForecastNoun = new CompatibleNoun() { noun = ForecastKeyword, result = ForecastNode, };
        ForecastKeyword.compatibleNouns = [ForecastNoun];

        compatibleNouns.Add(ForecastNoun);
        forecastNodes.Add(ForecastNode);
        forecastKeywords.Add(ForecastKeyword);

        TerminalNodeManager.ForecastTerminalNodes.Add(ForecastNode, level);
      });

      TerminalNodeManager.AddTerminalContent(forecastNodes, forecastKeywords);
      ForecastVerb.compatibleNouns = compatibleNouns.ToArray();

      // return (compatibleNouns, forecastNodes, forecastKeywords);
    }

    public static string GetForecast(SelectableLevel level)
    {
      string LevelName = ConfigHelper.GetAlphanumericName(level);
      ConsoleTable outputTable = new("Weather", "Default", "W2W", "Level");
      StringBuilder outputText = new();

      outputText.AppendLine($"Forecasting weather for {LevelName}");
      outputText.AppendLine($"Current weather: {WeatherManager.GetCurrentWeather(level).Name}\n");
      outputText.AppendLine("Weights for next days:\n");

      Dictionary<Weather, Dictionary<WeatherWeightType, int>> tomorrowWeights = [];

      WeatherManager.GetWeathers().ForEach(weather => tomorrowWeights.Add(weather, WeightsManager.GetWeatherWeightsAllTypes(level, weather)));
      int totalWeight = tomorrowWeights.Sum(x => x.Value.Values.Sum());
      tomorrowWeights = tomorrowWeights.OrderByDescending(x => x.Value[WeatherWeightType.Default]).ToDictionary(x => x.Key, x => x.Value);

      Logger.LogDebug($"Total weight for {LevelName}: {totalWeight}");

      foreach (var weather in tomorrowWeights)
      {
        outputTable.AddRow(
          $"<color=#{ColorHelper.ToHex(weather.Key.ColorGradient.topLeft)}>{weather.Key.NameShort.PadRight(12)}</color>",
          tomorrowWeights[weather.Key][WeatherWeightType.Default],
          tomorrowWeights[weather.Key][WeatherWeightType.WeatherToWeather],
          tomorrowWeights[weather.Key][WeatherWeightType.Level]
        );
      }

      outputText.Append(outputTable.ToStringCustomDecoration(true, false, false));

      return outputText.ToString();
    }
  }
}
