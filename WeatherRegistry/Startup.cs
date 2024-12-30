using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleTables;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TerminalStartPatch
  {
    internal static WeatherEffect[] vanillaEffectsArray { get; private set; } = null;
    internal static MrovLib.Logger Logger = new("WeatherRegistry", ConfigManager.LogStartup);
    internal static MrovLib.Logger WeightsLogger = new("WeatherRegistry", ConfigManager.LogStartupWeights);

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    internal static void StartOfRoundAwakePrefix(RoundManager __instance)
    {
      Logger.LogInfo("StartOfRoundAwakePrefix Patch");

      // Logger.LogDebug(GameNetworkManager.Instance);
      // Logger.LogDebug(GameNetworkManager.Instance.GetComponent<NetworkManager>());
      // Logger.LogDebug(WeatherSync.WeatherSyncPrefab);
      // Logger.LogDebug(WeatherSync.WeatherSyncPrefab.GetComponent<WeatherSync>());
      // Logger.LogDebug(WeatherSync.WeatherSyncPrefab.GetComponent<NetworkObject>());

      if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsHost)
      {
        Logger.LogDebug("Host detected, spawning WeatherSync");
        WeatherSync WeatherSyncPrefab = GameObject.Instantiate(WeatherSync.WeatherSyncPrefab).GetComponent<WeatherSync>();
        WeatherSyncPrefab.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    [HarmonyPriority(Priority.First)]
    public static bool TerminalPrefix(Terminal __instance)
    {
      if (WeatherManager.IsSetupFinished)
      {
        WeatherManager.IsSetupFinished = false;
      }

      return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    [HarmonyPriority(Priority.First)]
    public static void Postfix(Terminal __instance)
    {
      Logger.LogInfo("Terminal Start Patch");

      WeatherManager.Reset();
      EntranceTeleportPatch.isPlayerInside = false;

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

      WeatherManager.Weathers.Add(noneWeather);
      WeatherManager.NoneWeather = noneWeather;

      #endregion
      #region Vanilla weathers

      // Extend the weather enum to have the modded weathers
      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];
        Logger.LogInfo($"Effect: {effect.name}");

        LevelWeatherType weatherType = (LevelWeatherType)i;
        bool isVanilla = Defaults.VanillaWeathers.Contains(weatherType);

        WeatherType weatherTypeType = isVanilla ? WeatherType.Vanilla : WeatherType.Modded;
        Color weatherColor = isVanilla ? Defaults.VanillaWeatherColors[weatherType] : Color.blue;

        ImprovedWeatherEffect weatherEffect =
          new(effect.effectObject, effect.effectPermanentObject) { SunAnimatorBool = effect.sunAnimatorBool, };

        Weather weather =
          new(weatherType.ToString(), weatherEffect)
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

        WeatherManager.Weathers.Add(weather);
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

      // at this point we need to assing enum value for every registered modded weather that's not from lethallib
      int biggestKeyInModdedWeathersDictionary = Enum.GetValues(typeof(LevelWeatherType)).Length - 1;
      if (WeatherManager.ModdedWeatherEnumExtension.Count > 0)
      {
        biggestKeyInModdedWeathersDictionary = WeatherManager.ModdedWeatherEnumExtension.Keys.Max() + 1;
      }

      Logger.LogDebug(WeatherManager.ModdedWeatherEnumExtension.Count > 0);
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

        weatherList[entry.Key].effectObject?.SetActive(false);
        weatherList[entry.Key].effectPermanentObject?.SetActive(false);
      }
      TimeOfDay.Instance.effects = weatherList.ToArray();

      #endregion

      List<Weather> RegisteredWeathers = WeatherManager.RegisteredWeathers.Distinct().ToList();
      RegisteredWeathers.Sort((a, b) => a.VanillaWeatherType.CompareTo(b.VanillaWeatherType));

      for (int i = 0; i < RegisteredWeathers.Count; i++)
      {
        Logger.LogInfo($"Registered Weather: {RegisteredWeathers[i].Name}");

        Weather weather = RegisteredWeathers[i];
        WeatherManager.Weathers.Add(weather);
      }

      Logger.LogDebug($"Weathers: {WeatherManager.Weathers.Count}");

      List<SelectableLevel> levels = MrovLib.LevelHelper.SortedLevels;

      foreach (Weather weather in WeatherManager.Weathers)
      {
        Settings.ScreenMapColors.Add(weather.Name, weather.Color);
        weather.Init();

        List<SelectableLevel> LevelsToApply = [];

        if (weather.LevelFilteringOption == FilteringOption.Include)
        {
          LevelsToApply = weather.LevelFilters;
        }
        else if (weather.LevelFilteringOption == FilteringOption.Exclude)
        {
          LevelsToApply = levels.ToList();
          LevelsToApply.RemoveAll(level => weather.LevelFilters.Contains(level));
        }

        Logger.LogInfo($"Weather {weather.name} has {weather.LevelFilteringOption.ToString()} filtering option set up");

        AddWeatherToLevels(weather, levels, LevelsToApply);
      }

      #region Print possible weathers
      // print all possible weathers (defined in the config)

      var possibleWeathersTable = new ConsoleTable("Planet", "Random weathers");

      levels.ForEach(level =>
      {
        List<LevelWeatherType> randomWeathers = level.randomWeathers.Select(x => x.weatherType).ToList();
        randomWeathers.Sort();

        var stringifiedRandomWeathers = JsonConvert.SerializeObject(randomWeathers.Select(x => x.ToString()).ToList());
        possibleWeathersTable.AddRow(ConfigHelper.GetNumberlessName(level), stringifiedRandomWeathers);
      });

      Logger.LogInfo("Possible weathers:\n" + possibleWeathersTable.ToMinimalString());
      #endregion

      #region Print weights
      if (ConfigManager.LogStartupWeights.Value)
      {
        var weatherNames = WeatherManager.Weathers.Select(weather => weather.Name).ToArray();

        // default weights

        var defaultWeightsTable = new ConsoleTable(["Weather", "Default weight"]);
        WeatherManager.Weathers.ForEach(weather =>
        {
          defaultWeightsTable.AddRow(weather.Name, weather.Config.DefaultWeight.Value);
        });
        WeightsLogger.LogInfo("Default weights:\n" + defaultWeightsTable.ToMinimalString());

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
        WeightsLogger.LogInfo("Weather-weather weights:\n" + weatherToWeatherWeightsTable.ToMinimalString());

        // level weights
        string[] levelWeightsColumnNames = ["Level"];
        levelWeightsColumnNames = levelWeightsColumnNames.Concat(weatherNames).ToArray();

        var levelWeightsTable = new ConsoleTable(levelWeightsColumnNames);

        levels.ForEach(level =>
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
        WeightsLogger.LogInfo("Level weights:\n" + levelWeightsTable.ToMinimalString());
      }
      #endregion

      WeatherManager.IsSetupFinished = true;

      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

      if (!StartOfRound.Instance.IsHost)
      {
        Networking.WeatherLevelData.ApplyReceivedWeathers(WeatherSync.Instance.Weather);
      }

      EventManager.SetupFinished.Invoke();
    }

    static void AddWeatherToLevels(Weather weather, List<SelectableLevel> levels, List<SelectableLevel> LevelsToApply)
    {
      List<LevelWeatherVariables> levelWeatherVariables = [];
      weather.WeatherVariables.Clear();

      foreach (SelectableLevel level in levels)
      {
        Logger.LogDebug($"Level: {ConfigHelper.GetNumberlessName(level)}, weather: {weather.Name}");

        List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();

        LevelWeather levelWeather =
          new()
          {
            Weather = weather,
            Level = level,
            Variables = new()
          };

        RandomWeatherWithVariables randomWeather = null;

        // do that, but [continue] the loop when the result is null
        randomWeather = level.randomWeathers.FirstOrDefault(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);

        if (!InitializeRandomWeather(ref randomWeather, weather, level, ref randomWeathers, LevelsToApply))
        {
          Logger.LogDebug("randomWeather is null, skipping");
          continue;
        }

        levelWeather.Variables.Level = level;
        levelWeather.Variables.WeatherVariable1 = randomWeather?.weatherVariable ?? 1;
        levelWeather.Variables.WeatherVariable2 = randomWeather?.weatherVariable2 ?? 1;

        WeatherManager.LevelWeathers.Add(levelWeather);
        levelWeatherVariables.Add(levelWeather.Variables);
        weather.WeatherVariables.Add(level, levelWeather.Variables);
      }

      static bool InitializeRandomWeather(
        ref RandomWeatherWithVariables randomWeather,
        Weather weather,
        SelectableLevel level,
        ref List<RandomWeatherWithVariables> randomWeathers,
        List<SelectableLevel> LevelsToApply
      )
      {
        // TODO: rework this bit
        // because the shit condition above skips executing the blacklisting when the weather is possible
        // which (as you can imagine) is the exact fucking point
        // debugging this took me almost 2 hours

        // this has to execute only when the vanilla weather wasn't defined by a moon creator (cause they can)
        if (randomWeather == null && weather.Type == WeatherType.Vanilla)
        {
          return false;
        }
        // why would you make me do that
        else if (weather.Type == WeatherType.Clear)
        {
          randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = randomWeathers.ToArray();
          return false;
        }

        // remove all weathers from company moon, because that's the point
        if (level.PlanetName == "71 Gordion" && !LevelsToApply.Contains(level))
        {
          Logger.LogDebug($"Removing weather {weather.Name} from the company moon");

          randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = randomWeathers.ToArray();
          return false;
        }

        // Logger.LogDebug(
        //   $"Weather {weather.Name} has type {weather.Type.ToString()}, is level {level.name} in the list? : {LevelsToApply.Contains(level)}"
        // );

        switch (weather.Type)
        {
          case WeatherType.Vanilla:
          {
            // this will possibly break some things when it's not set up correctly
            // when that happens i'll send someone a link with this part of code
            // and blame them instead

            // vanilla weathers will not have an option of being added by configs
            // they have to be explicitly defined by moon makers
            // because that's how the variables work
            // but there will be an option to blacklist them on my end

            if (!LevelsToApply.Contains(level))
            {
              Logger.LogDebug($"Level {level.name} is not in the list of levels to apply weather to");

              if (randomWeather != null)
              {
                Logger.LogDebug($"Removing weather {weather.Name} from level {level.name}");
                randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
                level.randomWeathers = randomWeathers.ToArray();
              }

              return false;
            }

            return true;
          }
          case WeatherType.Modded:
          {
            if (randomWeather != null)
            {
              Logger.LogDebug($"Removing weather {weather.Name} from level {level.name} (added before lobby reload)");
              randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
            }

            Logger.LogDebug($"Adding modded weather {weather.Name}");

            if (!LevelsToApply.Contains(level))
            {
              Logger.LogDebug($"Level {level.name} is not in the list of levels to apply weather to");
              return false;
            }

            Logger.LogDebug(
              $"Injecting modded weather {weather.Name} for level {level.name} (variables {weather.Effect.DefaultVariable1}/{weather.Effect.DefaultVariable2})"
            );
            RandomWeatherWithVariables newWeather =
              new()
              {
                weatherType = weather.VanillaWeatherType,
                weatherVariable = weather.Effect.DefaultVariable1,
                weatherVariable2 = weather.Effect.DefaultVariable2
              };

            randomWeather = newWeather;
            randomWeathers.Add(newWeather);
            level.randomWeathers = randomWeathers.ToArray();
            break;
          }
        }
        return true;
      }
    }
  }
}
