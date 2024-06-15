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

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    internal static void StartOfRoundAwakePrefix(RoundManager __instance)
    {
      Plugin.logger.LogInfo("StartOfRoundAwakePrefix Patch");

      Plugin.logger.LogDebug(GameNetworkManager.Instance);
      Plugin.logger.LogDebug(GameNetworkManager.Instance.GetComponent<NetworkManager>());
      Plugin.logger.LogDebug(WeatherSync.WeatherSyncPrefab);
      Plugin.logger.LogDebug(WeatherSync.WeatherSyncPrefab.GetComponent<WeatherSync>());
      Plugin.logger.LogDebug(WeatherSync.WeatherSyncPrefab.GetComponent<NetworkObject>());

      if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsHost)
      {
        Plugin.logger.LogInfo("Host detected, spawning WeatherSync");
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
      Plugin.logger.LogInfo("Terminal Start Patch");

      WeatherManager.Reset();

      // GameObject.FindObjectsOfType<Weather>().ToList().ForEach(gameobject => GameObject.Destroy(gameobject));
      // GameObject.FindObjectsOfType<ImprovedWeatherEffect>().ToList().ForEach(gameobject => GameObject.Destroy(gameobject));

      WeatherEffect[] effects = TimeOfDay.Instance.effects;
      List<WeatherEffect> weatherList = effects.ToList();

      // this shit was not working lmao
      // a hacky bit of magic to reset vanilla effects array after reloading lobby
      // if (vanillaEffectsArray == null)
      // {
      //   vanillaEffectsArray = effects;
      // }
      // else
      // {
      //   Plugin.logger.LogWarning("Vanilla effects array is not null");
      //   effects = vanillaEffectsArray;
      // }

      if (effects == null || effects.Count() == 0)
      {
        Plugin.logger.LogWarning("Effects are null");
      }
      else
      {
        Plugin.logger.LogWarning($"Effects: {effects.Count()}");
      }

      Plugin.logger.LogMessage("Creating NoneWeather type");

      // Register clear weather as a weather
      Weather noneWeather =
        new(effect: new ImprovedWeatherEffect(null, null))
        {
          Type = WeatherType.Clear,
          Color = Defaults.VanillaWeatherColors[LevelWeatherType.None],
          VanillaWeatherType = LevelWeatherType.None,
          Origin = WeatherOrigin.Vanilla,
        };

      WeatherManager.Weathers.Add(noneWeather);
      WeatherManager.NoneWeather = noneWeather;

      // Extend the weather enum to have the modded weathers

      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];
        Plugin.logger.LogWarning($"Effect: {effect.name}");

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
          };

        WeatherManager.Weathers.Add(weather);
      }

      // Get all LethalLib weathers and add them to effects list
      if (Plugin.IsLethalLibLoaded)
      {
        Plugin.logger.LogWarning("Getting LethalLib Weathers");

        List<Weather> lethalLibWeathers = LethalLibPatch.ConvertLLWeathers();

        foreach (Weather weather in lethalLibWeathers)
        {
          Plugin.logger.LogWarning($"LethalLib Weather: {weather.Name}");

          WeatherManager.RegisteredWeathers.Add(weather);
        }
      }

      // at this point we need to assing enum value for every registered modded weather that's not from lethallib

      int biggestKeyInModdedWeathersDictionary = Enum.GetValues(typeof(LevelWeatherType)).Length - 1;
      if (WeatherManager.ModdedWeatherEnumExtension.Count > 0)
      {
        biggestKeyInModdedWeathersDictionary = WeatherManager.ModdedWeatherEnumExtension.Keys.Max() + 1;
      }
      Plugin.logger.LogDebug(WeatherManager.ModdedWeatherEnumExtension.Count > 0);
      Plugin.logger.LogDebug("Biggest key in modded weathers dictionary: " + biggestKeyInModdedWeathersDictionary);

      WeatherManager
        .RegisteredWeathers.Where(weather => weather.Origin == WeatherOrigin.WeatherRegistry)
        .ToList()
        .ForEach(weather =>
        {
          int newKey = biggestKeyInModdedWeathersDictionary;

          weather.VanillaWeatherType = (LevelWeatherType)newKey;
          // weather.Effect.VanillaWeatherType = (LevelWeatherType)newKey;
          WeatherManager.ModdedWeatherEnumExtension.Add(newKey, weather);
        });

      #region Extend the enum

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
      }
      TimeOfDay.Instance.effects = weatherList.ToArray();

      #endregion

      List<Weather> RegisteredWeathers = WeatherManager.RegisteredWeathers.Distinct().ToList();
      RegisteredWeathers.Sort((a, b) => a.VanillaWeatherType.CompareTo(b.VanillaWeatherType));

      for (int i = 0; i < RegisteredWeathers.Count; i++)
      {
        Plugin.logger.LogWarning($"Registered Weather: {RegisteredWeathers[i].Name}");

        Weather weather = RegisteredWeathers[i];
        WeatherManager.Weathers.Add(weather);
      }

      Plugin.logger.LogWarning($"Weathers: {WeatherManager.Weathers.Count}");

      List<SelectableLevel> levels = StartOfRound.Instance.levels.ToList();

      foreach (Weather weather in WeatherManager.Weathers)
      {
        Settings.ScreenMapColors.Add(weather.Name, weather.Color);
        weather.Init();

        if (weather.Type == WeatherType.Clear)
        {
          continue;
        }

        List<SelectableLevel> LevelsToApply = [];

        if (weather.LevelFilteringOption == FilteringOption.Include)
        {
          LevelsToApply = weather.LevelFilters;
        }
        else if (weather.LevelFilteringOption == FilteringOption.Exclude)
        {
          LevelsToApply = StartOfRound.Instance.levels.ToList();
          LevelsToApply.RemoveAll(level => weather.LevelFilters.Contains(level));
        }

        Plugin.logger.LogWarning($"Weather {weather.name} has {weather.LevelFilteringOption.ToString()} filtering option set up");

        AddWeatherToLevels(weather, levels, LevelsToApply);

        // Plugin.logger.LogInfo($"Registered weather: {weather.Name} under ID {weather.VanillaWeatherType}");
      }

      var possibleWeathersTable = new ConsoleTables.ConsoleTable("Planet", "Random weathers");

      levels.Sort((a, b) => ConfigHelper.GetNumberlessName(a).CompareTo(ConfigHelper.GetNumberlessName(b)));
      levels.ForEach(level =>
      {
        var stringifiedRandomWeathers = JsonConvert.SerializeObject(level.randomWeathers.Select(x => x.weatherType.ToString()).ToList());
        possibleWeathersTable.AddRow(ConfigHelper.GetNumberlessName(level), stringifiedRandomWeathers);
      });

      Plugin.logger.LogInfo("Possible weathers:\n" + possibleWeathersTable.ToMinimalString());

      WeatherManager.IsSetupFinished = true;

      EventManager.SetupFinished.Invoke();

      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

      if (!StartOfRound.Instance.IsHost)
      {
        WeatherSync.Instance.ApplyWeathers(WeatherSync.Instance.Weather);
      }
    }

    static void AddWeatherToLevels(Weather weather, List<SelectableLevel> levels, List<SelectableLevel> LevelsToApply)
    {
      List<LevelWeatherVariables> levelWeatherVariables = [];
      weather.WeatherVariables.Clear();

      foreach (SelectableLevel level in levels)
      {
        Plugin.logger.LogInfo($"Level: {level.name} ({ConfigHelper.GetNumberlessName(level)}), weather: {weather.Name}");

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
          Plugin.logger.LogDebug("Random Weather is null, skipping");
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
        else if (weather.Type == WeatherType.Clear)
        {
          randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = randomWeathers.ToArray();
          return false;
        }

        // remove all weathers from company moon, because that's the point
        if (level.PlanetName == "71 Gordion" && !LevelsToApply.Contains(level))
        {
          Plugin.logger.LogWarning($"Removing weather {weather.Name} from the company moon");

          randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
          level.randomWeathers = randomWeathers.ToArray();
          return false;
        }

        // Plugin.logger.LogDebug(
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
              Plugin.logger.LogInfo($"Level {level.name} is not in the list of levels to apply weather to");

              if (randomWeather != null)
              {
                Plugin.logger.LogInfo($"Removing weather {weather.Name} from level {level.name}");
                randomWeathers.RemoveAll(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);
                level.randomWeathers = randomWeathers.ToArray();
              }

              return false;
            }

            return true;
          }
          case WeatherType.Modded:
          {
            Plugin.logger.LogInfo($"Adding modded weather {weather.Name}");

            if (!LevelsToApply.Contains(level))
            {
              Plugin.logger.LogDebug($"Level {level.name} is not in the list of levels to apply weather to");
              return false;
            }

            Plugin.logger.LogInfo($"Injecting modded weather {weather.Name} for level {level.name}");
            RandomWeatherWithVariables newWeather =
              new()
              {
                weatherType = weather.VanillaWeatherType,
                weatherVariable = weather.Effect.DefaultVariable1,
                weatherVariable2 = weather.Effect.DefaultVariable2
              };

            Plugin.logger.LogInfo($"New Random Weather: {newWeather.weatherType}, {newWeather.weatherVariable}, {newWeather.weatherVariable2}");

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
