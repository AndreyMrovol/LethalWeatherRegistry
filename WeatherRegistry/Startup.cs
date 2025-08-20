using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleTables;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  public static class Startup
  {
    internal static WeatherEffect[] vanillaEffectsArray { get; private set; } = null;

    internal static Logger Logger = new("Startup", LoggingType.Developer);

    public static void Init(Terminal __instance)
    {
      Logger.LogInfo("Terminal Start Patch");

      WeatherManager.Reset();
      Settings.IsPlayerInside = false;
      Settings.IsGameStarted = false;

      EventManager.BeforeSetupStart.Invoke();

      WeatherEffect[] effects = TimeOfDay.Instance.effects;
      List<WeatherEffect> weatherList = effects.ToList();

      if (effects == null || effects.Count() == 0)
      {
        Logger.LogInfo("Effects are null");
      }
      else
      {
        Logger.LogInfo($"Effects: {effects.Count()}");
      }

      #region Remove incorrect weathers from RandomWeathers
      foreach (SelectableLevel level in MrovLib.LevelHelper.Levels)
      {
        List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();

        foreach (RandomWeatherWithVariables randomWeather in level.randomWeathers)
        {
          // if the value is not part of LevelWeatherType enum, remove RandomWeather
          if (!Enum.IsDefined(typeof(LevelWeatherType), randomWeather.weatherType))
          {
            randomWeathers.Remove(randomWeather);
            Plugin.logger.LogDebug($"Removing weather {randomWeather.weatherType} from level {level.name}");
          }
        }

        if (randomWeathers.Count != level.randomWeathers.Count())
        {
          level.randomWeathers = randomWeathers.ToArray();
        }
      }
      #endregion

      #region None weather

      Logger.LogInfo("Creating NoneWeather type");
      // Register clear weather as a weather
      Weather noneWeather =
        new(effect: new ImprovedWeatherEffect(null, null))
        {
          Type = WeatherType.Clear,
          Color = Defaults.VanillaWeatherColors[LevelWeatherType.None],
          VanillaWeatherType = LevelWeatherType.None,
          Origin = WeatherOrigin.Vanilla,
          Config =
          {
            WeatherToWeatherWeights = new(
              Defaults.VanillaWeatherToWeatherWeights.TryGetValue(LevelWeatherType.None, out string noneWeights)
                ? $"{String.Join(';', noneWeights)};"
                : Defaults.DefaultWeatherToWeatherWeights
            ),
          }
        };

      WeatherManager.WeathersDictionary.Add(LevelWeatherType.None, noneWeather);
      WeatherManager.NoneWeather = noneWeather;

      #endregion
      #region Vanilla weathers

      // Extend the weather enum to have the modded weathers
      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];

        LevelWeatherType weatherType = (LevelWeatherType)i;
        bool isVanilla = Defaults.VanillaWeathers.Contains(weatherType);

        string weatherName = weatherType.ToString();
        if (weatherName == "DustClouds")
        {
          weatherName = "Dust Clouds";
        }

        WeatherType weatherTypeType = isVanilla ? WeatherType.Vanilla : WeatherType.Modded;
        Color weatherColor = isVanilla ? Defaults.VanillaWeatherColors[weatherType] : Color.blue;

        ImprovedWeatherEffect weatherEffect =
          new(effect.effectObject, effect.effectPermanentObject) { SunAnimatorBool = effect.sunAnimatorBool, };

        Weather weather =
          new(weatherName, weatherEffect)
          {
            Type = weatherTypeType,
            Color = weatherColor,
            VanillaWeatherType = weatherType,
            Origin = WeatherOrigin.Vanilla,
            Config =
            {
              WeatherToWeatherWeights = new(
                Defaults.VanillaWeatherToWeatherWeights.TryGetValue(weatherType, out string vanillaWeatherWeights)
                  ? $"{String.Join(';', vanillaWeatherWeights)};"
                  : Defaults.DefaultWeatherToWeatherWeights
              ),
            }
          };

        WeatherManager.WeathersDictionary.Add(weather.VanillaWeatherType, weather);
      }

      #endregion

      #region LethalLib weathers

      // Get all LethalLib weathers and add them to effects list
      if (Plugin.IsLethalLibLoaded)
      {
        Logger.LogDebug("Getting LethalLib Weathers");

        List<Weather> lethalLibWeathers = LethalLibPatch.ConvertLLWeathers();

        foreach (Weather weather in lethalLibWeathers)
        {
          Logger.LogDebug($"LethalLib Weather: {weather.Name}");

          WeatherManager.RegisteredWeathers.Add(weather);
        }
      }

      #endregion

      #region Enum value assignment (hack)

      // assign enum value for every registered modded weather that's not from lethallib
      int biggestKeyInModdedWeathersDictionary = Enum.GetValues(typeof(LevelWeatherType)).Length - 1;
      if (WeatherManager.ModdedWeatherEnumExtension.Count > 0)
      {
        biggestKeyInModdedWeathersDictionary = WeatherManager.ModdedWeatherEnumExtension.Keys.Max() + 1;
      }

      Logger.LogDebug("Biggest key in modded weathers dictionary: " + biggestKeyInModdedWeathersDictionary);

      WeatherManager
        .RegisteredWeathers.Where(weather => weather.Origin == WeatherOrigin.WeatherRegistry || weather.Origin == WeatherOrigin.WeatherTweaks)
        .ToList()
        .ForEach(weather =>
        {
          int newKey = biggestKeyInModdedWeathersDictionary;

          weather.VanillaWeatherType = (LevelWeatherType)newKey;
          // weather.Effect.VanillaWeatherType = (LevelWeatherType)newKey;

          Logger.LogInfo($"Registering weather {weather.Name} under ID {newKey}");

          WeatherManager.ModdedWeatherEnumExtension.Add(newKey, weather);
          biggestKeyInModdedWeathersDictionary++;
        });

      #endregion

      #region Replace TimeOfDay effects

      // This is LethalLib's patch for extending the enum
      int highestIndex = 0;
      foreach (KeyValuePair<int, Weather> entry in WeatherManager.ModdedWeatherEnumExtension)
      {
        if (entry.Key > highestIndex)
        {
          highestIndex = entry.Key;
        }
      }

      // then we fill the list with nulls until we reach the highest index
      while (weatherList.Count <= highestIndex)
      {
        weatherList.Add(null);
      }

      GameObject WeatherParent = GameObject.Find("TimeAndWeather");

      // then we set the custom weathers at their index
      foreach (KeyValuePair<int, Weather> entry in WeatherManager.ModdedWeatherEnumExtension)
      {
        // if there's no defined ImprovedWeatherEffect for the weather, we create an empty one

        if (entry.Value.Effect == null)
        {
          entry.Value.Effect = new ImprovedWeatherEffect(null, null) { name = entry.Value.Name };
        }

        weatherList[entry.Key] = new WeatherEffect()
        {
          name = entry.Value.Name,
          effectObject = entry.Value.Effect.EffectObject,
          effectPermanentObject = entry.Value.Effect.WorldObject,
          sunAnimatorBool = entry.Value.Effect.SunAnimatorBool,
          effectEnabled = false,
          lerpPosition = false,
          transitioning = false
        };

        entry.Value.Effect.VanillaWeatherEffect = weatherList[entry.Key];

        if (weatherList[entry.Key].effectObject != null)
        {
          weatherList[entry.Key].effectObject.SetActive(false);
          weatherList[entry.Key].effectObject.transform.SetParent(WeatherParent.transform, false);
        }

        if (weatherList[entry.Key].effectPermanentObject != null)
        {
          weatherList[entry.Key].effectPermanentObject.SetActive(false);
          weatherList[entry.Key].effectPermanentObject.transform.SetParent(WeatherParent.transform, false);
        }
      }
      TimeOfDay.Instance.effects = weatherList.ToArray();

      #endregion

      List<Weather> RegisteredWeathers = WeatherManager.RegisteredWeathers.Distinct().ToList();
      RegisteredWeathers.Sort((a, b) => a.VanillaWeatherType.CompareTo(b.VanillaWeatherType));

      for (int i = 0; i < RegisteredWeathers.Count; i++)
      {
        Logger.LogInfo($"Registered Weather: {RegisteredWeathers[i].Name}");

        Weather weather = RegisteredWeathers[i];
        WeatherManager.WeathersDictionary.Add(weather.VanillaWeatherType, weather);
      }

      Logger.LogDebug($"Weathers: {WeatherManager.Weathers.Count}");

      foreach (Weather weather in WeatherManager.Weathers)
      {
        Settings.ScreenMapColors.Add(weather.Name, weather.Color);
        weather.Init();

        Logger.LogInfo($"Weather {weather.Name} has {weather.LevelFilteringOption} filtering option set up");

        List<SelectableLevel> LevelsToApply = [];

        if (weather.LevelFilteringOption == FilteringOption.Include)
        {
          LevelsToApply = weather.LevelFilters;
        }
        else if (weather.LevelFilteringOption == FilteringOption.Exclude)
        {
          LevelsToApply = MrovLib.LevelHelper.SortedLevels.ToList();
          LevelsToApply.RemoveAll(level => weather.LevelFilters.Contains(level));
        }

        AddWeatherToLevels(weather, LevelsToApply);
      }

      #region Print possible weathers
      // print all possible weathers (defined in the config)

      var possibleWeathersTable = new ConsoleTable("Planet", "Random weathers");

      MrovLib.LevelHelper.SortedLevels.ForEach(level =>
      {
        List<LevelWeatherType> randomWeathers = level.randomWeathers.Select(x => x.weatherType).ToList();
        randomWeathers.Sort();

        var stringifiedRandomWeathers = JsonConvert.SerializeObject(randomWeathers.Select(x => x.ToString()).ToList());
        possibleWeathersTable.AddRow(ConfigHelper.GetNumberlessName(level), stringifiedRandomWeathers);
      });

      Logger.LogInfo("Possible weathers:\n" + possibleWeathersTable.ToMinimalString());
      #endregion

      #region Print weights
      var weatherNames = WeatherManager.Weathers.Select(weather => weather.Name).ToArray();

      // default weights

      var defaultWeightsTable = new ConsoleTable(["Weather", "Default weight"]);
      WeatherManager.Weathers.ForEach(weather =>
      {
        defaultWeightsTable.AddRow(weather.Name, weather.Config.DefaultWeight.Value);
      });
      Logger.LogCustom("Default weights:\n" + defaultWeightsTable.ToMinimalString(), BepInEx.Logging.LogLevel.Info, LoggingType.Basic);

      // weather-weather weights
      string[] columnNames = ["From \\ To"];
      columnNames = columnNames.Concat(weatherNames).ToArray();

      var weatherToWeatherWeightsTable = new ConsoleTable(columnNames);

      WeatherManager.Weathers.ForEach(weather =>
      {
        var row = new List<string> { weather.Name };

        WeatherManager.Weathers.ForEach(weather2 =>
        {
          var (isWTW, weight) = weather2.GetWeatherToWeatherWeight(weather);

          if (isWTW)
          {
            row.Add(weight.ToString());
          }
          else
          {
            row.Add("X");
          }
        });

        weatherToWeatherWeightsTable.AddRow(row.ToArray());
      });
      Logger.LogCustom(
        "Weather-weather weights:\n" + weatherToWeatherWeightsTable.ToMinimalString(),
        BepInEx.Logging.LogLevel.Info,
        LoggingType.Basic
      );

      // level weights
      string[] levelWeightsColumnNames = ["Level"];
      levelWeightsColumnNames = levelWeightsColumnNames.Concat(weatherNames).ToArray();

      var levelWeightsTable = new ConsoleTable(levelWeightsColumnNames);

      MrovLib.LevelHelper.SortedLevels.ForEach(level =>
      {
        var row = new List<string> { ConfigHelper.GetNumberlessName(level) };

        WeatherManager.Weathers.ForEach(weather =>
        {
          if (weather.LevelWeights.TryGetValue(level, out int weight))
          {
            row.Add(weight.ToString());
          }
          else
          {
            row.Add("X");
          }
        });

        levelWeightsTable.AddRow(row.ToArray());
      });
      Logger.LogCustom("Level weights:\n" + levelWeightsTable.ToMinimalString(), BepInEx.Logging.LogLevel.Info, LoggingType.Basic);

      #endregion

      WeatherManager.IsSetupFinished = true;

      EventManager.SetupFinished.Invoke();

      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

      if (!StartOfRound.Instance.IsHost)
      {
        Networking.WeatherLevelData.ApplyReceivedWeathers(WeatherSync.Instance.Weather);
      }

      #region Compare the weather list between clients

      if (StartOfRound.Instance.IsHost)
      {
        WeatherSync.Instance.WeatherList = WeatherManager.GetWeatherList();
      }
      else
      {
        string hostWeatherList = WeatherSync.Instance.WeatherList.ToString();
        string localWeatherList = WeatherManager.GetWeatherList();

        if (hostWeatherList != localWeatherList)
        {
          Logger.LogError("Weathers are different between clients!");
          Logger.LogDebug($"Host: {hostWeatherList}");
          Logger.LogDebug($"Local: {localWeatherList}");
        }
      }

      #endregion
    }

    static void AddWeatherToLevels(Weather weather, List<SelectableLevel> LevelsToApply)
    {
      weather.WeatherVariables.Clear();
      StringBuilder weatherLog = new();
      weatherLog.Append($"Weather: {weather.Name}");

      foreach (SelectableLevel level in MrovLib.LevelHelper.SortedLevels)
      {
        weatherLog.AppendLine();

        weatherLog.Append($"{ConfigHelper.GetNumberlessName(level)}: ");

        // Get level data
        List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();
        RandomWeatherWithVariables randomWeather = level.randomWeathers.FirstOrDefault(rw => rw.weatherType == weather.VanillaWeatherType);
        bool isLevelToApply = LevelsToApply.Contains(level);

        // CASE 1: Skip vanilla weather that wasn't defined by a moon creator
        if (randomWeather == null && weather.Type == WeatherType.Vanilla)
        {
          weatherLog.Append("Vanilla weather not defined by moon creator");
          continue;
        }

        // CASE 2: Handle Clear weather type
        // Keep Clear weather for vanilla algorithm as it increases Clear weather chance
        if (weather.Type == WeatherType.Clear && Settings.WeatherSelectionAlgorithm != WeatherCalculation.VanillaAlgorithm)
        {
          randomWeathers.RemoveAll(rw => rw.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = GetRandomWeathersWithVariables(randomWeathers);
          weatherLog.Append("Clear weather type removed");
          continue;
        }

        // CASE 3: Remove weather from company moons if not explicitly included
        if (MrovLib.LevelHelper.CompanyMoons.Contains(level) && !isLevelToApply)
        {
          // Logger.LogDebug($"Removing weather {weather.Name} from the company moon {level.name}");
          randomWeathers.RemoveAll(rw => rw.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = GetRandomWeathersWithVariables(randomWeathers);
          weatherLog.Append("Removed from company moon");
          continue;
        }

        // CASE 4: Level not in list of levels to apply this weather to
        if (!isLevelToApply)
        {
          if (randomWeather != null)
          {
            // Logger.LogDebug($"Removing weather {weather.Name} from level {level.name}");
            randomWeathers.RemoveAll(rw => rw.weatherType == weather.VanillaWeatherType);
            level.randomWeathers = GetRandomWeathersWithVariables(randomWeathers);
          }
          weatherLog.Append("Level not in list to apply weather to");
          continue;
        }

        // CASE 5: Handle different weather types
        if (weather.Type <= WeatherType.Vanilla)
        {
          // For vanilla weather, we don't need to do anything else
          // It's already in the level's randomWeathers
          weatherLog.Append(
            "Weather is already there"
              + (randomWeather != null ? $" (variables {randomWeather.weatherVariable}/{randomWeather.weatherVariable2})" : "")
          );
          continue;
        }
        else if (weather.Type == WeatherType.Modded)
        {
          // For modded weather, add it to the level
          // this should never happen, but just in case
          if (randomWeather != null)
          {
            weatherLog.Append($"Removing existing weather (added before lobby reload)");
            randomWeathers.RemoveAll(rw => rw.weatherType == weather.VanillaWeatherType);
          }

          weatherLog.Append($"Injecting modded weather (variables {weather.Effect.DefaultVariable1}/{weather.Effect.DefaultVariable2})");

          RandomWeatherWithVariables newWeather =
            new()
            {
              weatherType = weather.VanillaWeatherType,
              weatherVariable = weather.Effect.DefaultVariable1,
              weatherVariable2 = weather.Effect.DefaultVariable2
            };

          randomWeathers.Add(newWeather);

          level.randomWeathers = GetRandomWeathersWithVariables(randomWeathers);
        }
      }

      weatherLog.AppendLine();
      Logger.LogDebug(weatherLog.ToString());
    }

    private static ImprovedRandomWeatherWithVariables[] GetRandomWeathersWithVariables(List<RandomWeatherWithVariables> randomWeathers)
    {
      ImprovedRandomWeatherWithVariables[] improvedWeathers = randomWeathers
        .Select(rw => new ImprovedRandomWeatherWithVariables
        {
          weatherType = rw.weatherType,
          weatherVariable = rw.weatherVariable,
          weatherVariable2 = rw.weatherVariable2
        })
        .ToArray();
      return improvedWeathers;
    }
  }
}
