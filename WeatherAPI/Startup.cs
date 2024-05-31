using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace WeatherAPI.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TerminalStartPatch
  {
    internal static WeatherEffect[] vanillaEffectsArray { get; private set; } = null;

    [HarmonyPatch(typeof(RoundManager), "Awake")]
    [HarmonyPostfix]
    internal static void RoundManagerAwakePostfix(RoundManager __instance)
    {
      Plugin.logger.LogInfo("RoundManager Awake Patch");

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

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    [HarmonyPriority(Priority.First)]
    public static void Postfix(Terminal __instance)
    {
      Plugin.logger.LogInfo("Terminal Start Patch");

      WeatherManager.Reset();

      WeatherEffect[] effects = TimeOfDay.Instance.effects;
      List<WeatherEffect> weatherList = effects.ToList();

      // a hacky bit of magic to reset vanilla effects array after reloading lobby
      if (vanillaEffectsArray == null)
      {
        vanillaEffectsArray = effects;
      }
      else
      {
        Plugin.logger.LogWarning("Vanilla effects array is not null");
        effects = vanillaEffectsArray;
      }

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
      Weather noneWeather = Weather.CreateCanvasWeather();

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
        biggestKeyInModdedWeathersDictionary = WeatherManager.ModdedWeatherEnumExtension.Keys.Max();
      }

      WeatherManager
        .RegisteredWeathers.Where(weather => weather.Origin == WeatherOrigin.WeatherAPI)
        .ToList()
        .ForEach(weather =>
        {
          int newKey = biggestKeyInModdedWeathersDictionary;

          weather.VanillaWeatherType = (LevelWeatherType)newKey;
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
        switch (weather.Type)
        {
          case WeatherType.Clear:
          case WeatherType.Vanilla:
          case WeatherType.Modded:
          default:
            break;
        }

        WeatherManager.Weathers.Add(weather);
      }

      Plugin.logger.LogWarning($"Weathers: {WeatherManager.Weathers.Count}");

      List<SelectableLevel> levels = StartOfRound.Instance.levels.ToList();

      foreach (Weather weather in WeatherManager.Weathers)
      {
        if (weather.Type == WeatherType.Clear)
        {
          continue;
        }

        AddWeatherToLevels(weather, ref levels);
        SaveWeatherColor(weather);
      }

      WeatherManager.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    static void SaveWeatherColor(Weather weather)
    {
      Settings.ScreenMapColors.Add(weather.Name, weather.Color);
    }

    static void AddWeatherToLevels(Weather weather, ref List<SelectableLevel> levels)
    {
      List<LevelWeatherVariables> levelWeatherVariables = [];

      foreach (SelectableLevel level in levels)
      {
        Plugin.logger.LogInfo($"Level: {level.name}, weather: {weather.Name}");

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

        Plugin.logger.LogWarning(
          $"Random Weather for weather {weather.Name} for level {level.PlanetName}: " + randomWeather?.weatherType.ToString() ?? "null"
        );

        if (randomWeather == null && !InitializeRandomWeather(ref randomWeather, ref weather, level, ref randomWeathers))
          continue;

        levelWeather.Variables.Level = level;
        levelWeather.Variables.WeatherVariable1 = randomWeather?.weatherVariable ?? 1;
        levelWeather.Variables.WeatherVariable2 = randomWeather?.weatherVariable2 ?? 1;

        WeatherManager.LevelWeathers.Add(levelWeather);
        levelWeatherVariables.Add(levelWeather.Variables);
        weather.WeatherVariables.Add(level, levelWeather.Variables);
      }

      static bool InitializeRandomWeather(
        ref RandomWeatherWithVariables randomWeather,
        ref Weather weather,
        SelectableLevel level,
        ref List<RandomWeatherWithVariables> randomWeathers
      )
      {
        switch (weather.Type)
        {
          case WeatherType.Vanilla:
          {
            Plugin.logger.LogWarning("Random Weather is null");
            return false;
          }
          case WeatherType.Modded:
          {
            Plugin.logger.LogWarning($"Random Weather is null, injecting modded weather {weather.Name}");

            if (weather.LevelBlacklist.Contains(level.name))
              return false;
            Plugin.logger.LogInfo($"Injecting modded weather {weather.Name} for level {level.name}");
            RandomWeatherWithVariables newWeather = new RandomWeatherWithVariables()
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

      WeatherManager.IsSetupFinished = true;

      if (StartOfRound.Instance.IsHost)
      {
        StartOfRound.Instance.SetPlanetsWeather();
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
      }
      else
      {
        WeatherSync.Instance.ApplyWeathers(WeatherSync.Instance.Weather);
      }
    }
  }
}
