using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace WeatherAPI.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public class TerminalStartPatch
  {
    internal static WeatherEffect[] vanillaEffectsArray { get; private set; } = null;

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

      List<LevelWeatherType> VanillaWeathers =
      [
        LevelWeatherType.None,
        LevelWeatherType.DustClouds,
        LevelWeatherType.Foggy,
        LevelWeatherType.Rainy,
        LevelWeatherType.Stormy,
        LevelWeatherType.Flooded,
        LevelWeatherType.Eclipsed
      ];

      Dictionary<LevelWeatherType, Color> VanillaWeatherColors =
        new()
        {
          { LevelWeatherType.None, new Color(0.41f, 1f, 0.42f, 1f) },
          { LevelWeatherType.DustClouds, new Color(0.41f, 1f, 0.42f, 1f) },
          { LevelWeatherType.Foggy, new Color(1f, 0.86f, 0f, 1f) },
          { LevelWeatherType.Rainy, new Color(1f, 0.86f, 0f, 1f) },
          { LevelWeatherType.Stormy, new Color(1f, 0.57f, 0f, 1f) },
          { LevelWeatherType.Flooded, new Color(1f, 0.57f, 0f, 1f) },
          { LevelWeatherType.Eclipsed, new Color(1f, 0f, 0f, 1f) }
        };

      Plugin.logger.LogMessage("Creating NoneWeather type");

      // Register clear weather as a weather
      Weather noneWeather =
        new("None", new ImprovedWeatherEffect(null, null))
        {
          Type = WeatherType.Clear,
          Color = VanillaWeatherColors[LevelWeatherType.None],
          VanillaWeatherType = LevelWeatherType.None,
          Origin = WeatherOrigin.Vanilla,
        };
      WeatherManager.Weathers.Add(noneWeather);

      // Extend the weather enum to have the modded weathers

      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];
        Plugin.logger.LogWarning($"Effect: {effect.name}");

        LevelWeatherType weatherType = (LevelWeatherType)i;
        bool isVanilla = VanillaWeathers.Contains(weatherType);

        WeatherType weatherTypeType = isVanilla ? WeatherType.Vanilla : WeatherType.Modded;
        Color weatherColor = isVanilla ? VanillaWeatherColors[weatherType] : Color.blue;

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

        if (weather.Type == WeatherType.Modded) { }

        WeatherManager.Weathers.Add(weather);
      }

      Plugin.logger.LogWarning($"Weathers: {WeatherManager.Weathers.Count}");

      List<SelectableLevel> levels = StartOfRound.Instance.levels.ToList();

      foreach (Weather weather in WeatherManager.Weathers)
      {
        List<LevelWeatherVariables> levelWeatherVariables = [];

        if (weather.Type == WeatherType.Clear)
        {
          continue;
        }

        foreach (SelectableLevel level in levels)
        {
          Plugin.logger.LogInfo($"Level: {level.name}, weather: {weather.Name}");

          List<RandomWeatherWithVariables> randomWeathers = level.randomWeathers.ToList();

          // i'm leaving it at that for now

          // if (level.PlanetName == "71 Gordion")
          // {
          //   Plugin.logger.LogWarning("Removing all weathers from the company moon");

          //   randomWeathers.Clear();
          //   level.randomWeathers = randomWeathers.ToArray();
          //   continue;
          // }

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

          if (randomWeather == null)
          {
            if (weather.Type == WeatherType.Vanilla)
            {
              Plugin.logger.LogWarning("Random Weather is null");
              continue;
            }
            else if (weather.Type == WeatherType.Modded)
            {
              Plugin.logger.LogWarning($"Random Weather is null, injecting modded weather {weather.Name}");

              if (!weather.LevelBlacklist.Contains(level.name))
              {
                Plugin.logger.LogInfo($"Injecting modded weather {weather.Name} for level {level.name}");
                RandomWeatherWithVariables newWeather = new RandomWeatherWithVariables()
                {
                  weatherType = weather.VanillaWeatherType,
                  weatherVariable = weather.Effect.DefaultVariable1,
                  weatherVariable2 = weather.Effect.DefaultVariable2
                };

                Plugin.logger.LogInfo(
                  $"New Random Weather: {newWeather.weatherType}, {newWeather.weatherVariable}, {newWeather.weatherVariable2}"
                );

                randomWeathers.Add(newWeather);
                level.randomWeathers = randomWeathers.ToArray();
              }
              else
              {
                continue;
              }
            }
          }

          levelWeather.Variables.Level = level;
          levelWeather.Variables.WeatherVariable1 = randomWeather?.weatherVariable ?? 1;
          levelWeather.Variables.WeatherVariable2 = randomWeather?.weatherVariable2 ?? 1;

          WeatherManager.LevelWeathers.Add(levelWeather);
          levelWeatherVariables.Add(levelWeather.Variables);
          weather.WeatherVariables.Add(level, levelWeather.Variables);
        }
      }

      WeatherManager.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
