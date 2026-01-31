using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleTables;
using MrovLib;
using MrovLib.Definitions;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;
using WeatherRegistry.Managers;
using WeatherRegistry.Utils;

namespace WeatherRegistry.Modules
{
  public static class Forecasts
  {
    private static readonly Logger Logger = new("Forecast", LoggingType.Debug);

    public class WeatherForecastCommandNode(string Name) : RegistryCommandNode(Name)
    {
      public override string Execute(string[] args)
      {
        string levelName;
        SelectableLevel level;

        if (args.Length < 1)
        {
          levelName = "";
        }
        else
        {
          levelName = args[0];
        }

        if (levelName == "")
        {
          level = StartOfRound.Instance.currentLevel;
        }
        else
        {
          level = ConfigHelper.ConvertStringToLevels(levelName).First();
        }

        string LevelName = ConfigHelper.GetAlphanumericName(level);
        ConsoleTable outputTable = new("Weather", "Default", "W2W", "Level", "% chance");
        StringBuilder outputText = new();

        outputText.AppendLine($"Forecasting weather for {LevelName}");
        outputText.AppendLine($"Current weather: {WeatherManager.GetCurrentWeather(level).Name}\n");
        outputText.AppendLine("Weights for tomorrow:\n");

        Dictionary<Weather, Dictionary<WeatherWeightType, int>> tomorrowWeights = [];

        WeatherManager
          .GetWeathers()
          .ForEach(weather =>
          {
            tomorrowWeights.Add(weather, WeightsManager.GetWeatherWeightsAllTypes(level, weather));
          });

        int totalWeight = tomorrowWeights.Sum(x => x.Value.Values.Sum());
        tomorrowWeights = tomorrowWeights.OrderByDescending(x => x.Value[WeatherWeightType.Default]).ToDictionary(x => x.Key, x => x.Value);

        Logger.LogDebug($"Total weight for {LevelName}: {totalWeight}");

        Dictionary<Weather, int> tomorrowWeightsSorted = tomorrowWeights
          .ToDictionary(
            x => x.Key,
            x =>
            {
              if (x.Value[WeatherWeightType.Level] != 0)
              {
                return x.Value[WeatherWeightType.Level];
              }
              else if (x.Value[WeatherWeightType.WeatherToWeather] != 0)
              {
                return x.Value[WeatherWeightType.WeatherToWeather];
              }
              else
              {
                return x.Value[WeatherWeightType.Default];
              }
            }
          )
          .OrderByDescending(x => x.Value)
          .ToDictionary(x => x.Key, x => x.Value);

        foreach (var weather in tomorrowWeightsSorted)
        {
          string weatherName = weather.Key.NameShort.PadRight(12);

          if (!CanTheWeatherBeThere(level, weather.Key))
          {
            weatherName = $"<s>{weatherName}</s>";
          }

          outputTable.AddRow(
            $"<color=#{ColorConverter.ToHex(weather.Key.ColorGradient.topLeft)}>{weatherName}</color>",
            tomorrowWeights[weather.Key][WeatherWeightType.Default],
            tomorrowWeights[weather.Key][WeatherWeightType.WeatherToWeather],
            tomorrowWeights[weather.Key][WeatherWeightType.Level],
            $"{weather.Value * 100f / totalWeight:0.00}%"
          );
        }

        outputText.Append(outputTable.ToStringCustomDecoration(true, false, false));

        return outputText.ToString();
      }

      public bool CanTheWeatherBeThere(SelectableLevel level, Weather weather)
      {
        List<LevelWeatherType> levelWeathers = WeightsManager.GetPlanetPossibleWeathers(level);
        return levelWeathers.Contains(weather.VanillaWeatherType);
      }
    }
  }
}
