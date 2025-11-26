using System.Collections.Generic;
using System.Linq;
using System.Text;
using MrovLib;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;

namespace WeatherRegistry.Algorithms;

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

    Logger.LogCustom(
      $"Levels: {string.Join(';', levels.Select(level => ConfigHelper.GetAlphanumericName(level)))}",
      BepInEx.Logging.LogLevel.Debug,
      LoggingType.Developer
    );

    StringBuilder weatherLog = new();

    foreach (SelectableLevel level in levels)
    {
      weatherLog.AppendLine();

      WeatherCalculation.previousDayWeather[level.PlanetName] = level.currentWeather;

      // possible weathers taken from level.randomWeathers (modified by me)
      // use random for seeded randomness

      int longestPlanetName = LevelHelper.LongestPlanetName.Length;
      bool isDebugLoggingEnabled = ConfigManager.LoggingLevels.Value >= LoggingType.Debug;

      if (!isDebugLoggingEnabled)
      {
        weatherLog.Append($"\t");
      }

      weatherLog.Append($"[{ConfigHelper.GetAlphanumericName(level)}] ".PadRight(isDebugLoggingEnabled ? 0 : longestPlanetName + 4));

      if (level.overrideWeather)
      {
        weatherLog.Append($"Override weather: {level.overrideWeatherType}");
        Weather overrideWeather = WeatherManager.GetWeather(level.overrideWeatherType);

        NewWeather[level] = overrideWeather.VanillaWeatherType;
        EventManager.WeatherChanged.Invoke((level, overrideWeather));

        continue;
      }

      NewWeather[level] = LevelWeatherType.None;

      Utils.WeightHandler<Weather, WeatherWeightType> possibleWeathers = WeatherManager.GetPlanetWeightedList(level);
      Weather selectedWeather = possibleWeathers.Random();

      NewWeather[level] = selectedWeather.VanillaWeatherType;
      EventManager.WeatherChanged.Invoke((level, selectedWeather));

      weatherLog.Append(
        $"Selected weather: {selectedWeather.Name} ({(float)possibleWeathers.Get(selectedWeather) / possibleWeathers.Sum * 100:F2}% chance)"
      );

      if (isDebugLoggingEnabled)
      {
        weatherLog.AppendLine();

        weatherLog.AppendLine($"PreviousDayWeather: {WeatherCalculation.previousDayWeather[level.PlanetName]}");
        weatherLog.AppendLine($"Possible weathers: [{string.Join(", ", level.randomWeathers.Select(rw => rw.weatherType))}]");
        weatherLog.AppendLine($"Used weights: ");
        foreach (Weather weather in possibleWeathers.Keys)
        {
          weatherLog.AppendLine(
            $"  {weather.Name} has {possibleWeathers.GetOrigin(weather).ToString().ToLowerInvariant()} weight ({possibleWeathers.Get(weather)})"
          );
        }
      }
    }

    Logger.LogMessage(weatherLog.ToString());

    return NewWeather;
  }
}
