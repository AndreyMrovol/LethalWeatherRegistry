using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;
using MrovLib;
using WeatherRegistry.Definitions;
using WeatherRegistry.Editor;

namespace WeatherRegistry.Managers
{
  public static class RandomWeathersManager
  {
    private static readonly Logger Logger = new("RandomWeathersManager", LoggingType.Debug);

    public static Dictionary<SelectableLevel, List<ImprovedRandomWeatherWithVariables>> RandomWeatherPairs = [];
    public static Dictionary<SelectableLevel, List<LevelWeatherType>> BlacklistedWeathers = [];

    public static void Init()
    {
      // Load all ImprovedRandomWeathers assets
      foreach (ImprovedRandomWeathers randomWeathers in Plugin.AssetBundleManager.GetLoadedAssets<ImprovedRandomWeathers>())
      {
        if (randomWeathers == null || randomWeathers.LevelWeathers == null || randomWeathers.LevelWeathers.Length == 0)
        {
          Logger.LogWarning($"ImprovedRandomWeathers {randomWeathers.name} is null or has no entries!");
          continue;
        }

        foreach (var ImpRandomWeather in randomWeathers.LevelWeathers)
        {
          if (ImpRandomWeather.LevelName == String.Empty)
          {
            Logger.LogWarning($"LevelWeatherMatcher in ImprovedRandomWeathers {randomWeathers.name} has no Level assigned, skipping!");
            continue;
          }

          SelectableLevel level = ConfigHelper.ConvertStringToLevels(ImpRandomWeather.LevelName).FirstOrDefault();
          if (level == null)
          {
            Logger.LogWarning($"Level {ImpRandomWeather.LevelName} not found, skipping!");
            continue;
          }

          if (LevelHelper.IsVanillaLevel(level))
          {
            Logger.LogInfo($"Level {level.PlanetName} is a vanilla level, skipping checking defined RandomWeathers!");
            continue;
          }

          if (ImpRandomWeather.Weathers == null || ImpRandomWeather.Weathers.Length == 0)
          {
            Logger.LogInfo($"No weathers defined for level {level.PlanetName} in ImprovedRandomWeathers {randomWeathers.name}, skipping.");
            continue;
          }

          RandomWeatherPairs.TryGetValue(level, out var randomWeathersList);
          BlacklistedWeathers.TryGetValue(level, out var blacklistedWeathersList);

          if (randomWeathersList == null)
          {
            randomWeathersList = [];
            RandomWeatherPairs[level] = randomWeathersList;
          }

          if (blacklistedWeathersList == null)
          {
            blacklistedWeathersList = [];
            BlacklistedWeathers[level] = blacklistedWeathersList;
          }

          RandomWeatherEntry[] weathers = ImpRandomWeather.Weathers;

          foreach (var weatherEntry in weathers)
          {
            if (string.IsNullOrEmpty(weatherEntry.WeatherName))
            {
              Logger.LogWarning($"RandomWeatherEntry in ImprovedRandomWeathers {randomWeathers.name} has no WeatherName assigned, skipping!");
              continue;
            }

            var resolvedWeather = ConfigHelper.ResolveStringToWeather(weatherEntry.WeatherName);
            if (resolvedWeather == null)
            {
              Logger.LogWarning($"Could not resolve weather {weatherEntry.WeatherName} for level {level.PlanetName}, skipping!");
              continue;
            }

            if (weatherEntry.Blacklist)
            {
              blacklistedWeathersList.Add(resolvedWeather.VanillaWeatherType);
              Logger.LogInfo($"Weather {weatherEntry.WeatherName} is blacklisted for level {level.PlanetName}!");
              continue;
            }

            Logger.LogCustom(
              $"Adding random weather {resolvedWeather.Name} to level {level.PlanetName} with variables ({weatherEntry.Variable};{weatherEntry.Variable2})",
              LogLevel.Info,
              LoggingType.Developer
            );

            randomWeathersList.Add(
              new ImprovedRandomWeatherWithVariables()
              {
                weatherType = resolvedWeather.VanillaWeatherType,
                weatherVariable = weatherEntry.Variable,
                weatherVariable2 = weatherEntry.Variable2,
              }
            );
          }
        }
      }
    }
  }
}
