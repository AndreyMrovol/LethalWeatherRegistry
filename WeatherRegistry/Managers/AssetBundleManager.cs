using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using MrovLib;
using TMPro;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Editor;
using WeatherRegistry.Enums;
using WeatherRegistry.Helpers;

namespace WeatherRegistry.Managers
{
  public class AssetBundleManager : MrovLib.AssetBundleLoaderManager
  {
    public AssetBundleManager()
      : base()
    {
      BundleExtensions = ["weatherbundle"];
      Logger = new WeatherRegistry.Logger("AssetBundleManager", LoggingType.Debug);

      AssetBundleLoadersByType.AddRange(
        new Dictionary<System.Type, AssetBundleLoader>
        {
          { typeof(Editor.WeatherDefinition), new AssetBundleLoader<Editor.WeatherDefinition>() },
          { typeof(EffectOverride), new AssetBundleLoader<EffectOverride>() },
          { typeof(PlanetNameOverride), new AssetBundleLoader<PlanetNameOverride>() },
          { typeof(ModdedWeathersMatcher), new AssetBundleLoader<ModdedWeathersMatcher>() },
        }
      );
    }

    public override void ConvertLoadedAssets()
    {
      Plugin.logger.LogInfo(GetLoadedAssets<Editor.WeatherDefinition>().Count + " WeatherDefinition assets loaded.");

      foreach (Editor.WeatherDefinition WeatherDefinition in GetLoadedAssets<Editor.WeatherDefinition>())
      {
        if (WeatherDefinition.Effect == null)
        {
          Logger.LogWarning($"WeatherDefinition {WeatherDefinition.Name} has no Effect assigned - skipping!");
          continue;
        }

        GameObject effectObject = null;
        if (WeatherDefinition.Effect.EffectObject != null)
        {
          effectObject = GameObject.Instantiate(WeatherDefinition.Effect.EffectObject);
          if (effectObject != null)
          {
            effectObject.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(effectObject);
          }
        }

        GameObject effectPermanentObject = null;
        if (WeatherDefinition.Effect.WorldObject != null)
        {
          effectPermanentObject = GameObject.Instantiate(WeatherDefinition.Effect.WorldObject);
          if (effectPermanentObject != null)
          {
            effectPermanentObject.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(effectPermanentObject);
          }
        }

        ImprovedWeatherEffect newImprovedWeatherEffect =
          new(effectObject, effectPermanentObject)
          {
            SunAnimatorBool = WeatherDefinition.Effect.SunAnimatorBool,
            DefaultVariable1 = WeatherDefinition.Effect.DefaultVariable1,
            DefaultVariable2 = WeatherDefinition.Effect.DefaultVariable2
          };

        if (WeatherDefinition.ColorGradient == null)
        {
          WeatherDefinition.ColorGradient = ColorHelper.ToTMPColorGradient(WeatherDefinition.Color);
        }

        Weather weather =
          new(WeatherDefinition.Name, newImprovedWeatherEffect)
          {
            ColorGradient = WeatherDefinition.ColorGradient,
            Origin = WeatherOrigin.WeatherRegistry,
            Type = WeatherType.Modded,
            Config = WeatherDefinition.Config.CreateFullConfig(),
          };

        Plugin.logger.LogInfo($"Registering weather: {weather}");
        WeatherManager.RegisterWeather(weather);
      }

      List<PlanetNameOverride> LoadedPlanetNameOverrides = GetLoadedAssets<PlanetNameOverride>();

      // Load all EffectOverride assets
      foreach (EffectOverride effectOverride in GetLoadedAssets<EffectOverride>())
      {
        if (effectOverride == null || string.IsNullOrEmpty(effectOverride.weatherName))
        {
          Logger.LogWarning("EffectOverride is null or has no weatherName, skipping.");
          continue;
        }

        Weather weather = ConfigHelper.ResolveStringToWeather(effectOverride.weatherName);
        if (weather == null)
        {
          Logger.LogWarning($"Weather {effectOverride.weatherName} not found, skipping EffectOverride.");
          continue;
        }

        SelectableLevel[] levels = ConfigHelper.ConvertStringToLevels(effectOverride.levelName);

        ImprovedWeatherEffect overrideEffect = effectOverride.OverrideEffect;
        PlanetNameOverride planetNameOverride = LoadedPlanetNameOverrides.Where(o => o.effectOverride == overrideEffect).First();

        foreach (SelectableLevel level in levels)
        {
          WeatherEffectOverride newOverride =
            new(weather, level, overrideEffect, effectOverride.weatherDisplayName, effectOverride.weatherDisplayColor);

          if (planetNameOverride != null && !string.IsNullOrEmpty(planetNameOverride.newPlanetName))
          {
            OverridesManager.PlanetOverrideNames.Add(newOverride, planetNameOverride.newPlanetName);
          }
        }

        // Load all ModdedWeathersMatcher assets
        foreach (ModdedWeathersMatcher matcher in GetLoadedAssets<ModdedWeathersMatcher>())
        {
          if (matcher == null || matcher.Weathers == null || matcher.Level == null || matcher.Weathers.Length == 0)
          {
            Logger.LogWarning($"ModdedWeathersMatcher {matcher.name} is null or has no weathers!");
            continue;
          }

          SelectableLevel level = matcher.Level;

          matcher
            .Weathers.ToList()
            .ForEach(weather =>
            {
              Weather matchedWeather = ConfigHelper.ResolveStringToWeather(weather.Name);
              int matcherWeight = weather.DefaultLevelWeight;
              if (matchedWeather == null)
              {
                Logger.LogDebug($"Weather {weather.Name} not found!");
                return;
              }

              LevelRarity[] WeatherConfigEntryName = matchedWeather.Config.LevelWeights.Value;

              List<SelectableLevel> levels = WeatherConfigEntryName.Select(rarity => rarity.Level).ToList();
              List<string> levelNames = levels.Select(planet => planet.PlanetName).ToList();

              if (!levelNames.Contains(level.PlanetName))
              {
                // if the level is not in the list, add it
                Logger.LogDebug($"Level {level.PlanetName} not found in {matchedWeather.Name}");

                // check if the config entry is enabled
                if (matchedWeather.Config.LevelWeights.ConfigEntryActive)
                {
                  Logger.LogDebug($"Adding {level.PlanetName} to {matchedWeather.Name} with weight {matcherWeight}");
                  matchedWeather.Config.LevelWeights.ConfigEntry.Value += $";{ConfigHelper.GetAlphanumericName(level)}@{matcherWeight};";
                }
              }
              else
              {
                Logger.LogDebug($"Level {level.PlanetName} already exists in {matchedWeather.Name} LevelWeights, skipping.");
              }
            });
        }
      }
    }
  }
}
