using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Editor;
using WeatherRegistry.Helpers;

namespace WeatherRegistry.Managers
{
  public class AssetBundleManager : MrovLib.AssetBundleLoaderManager
  {
    public AssetBundleManager()
      : base()
    {
      BundleExtensions = ["weatherbundle"];

      AssetBundleLoadersByType.AddRange(
        new Dictionary<System.Type, AssetBundleLoader>
        {
          { typeof(WeatherDefinition), new AssetBundleLoader<WeatherDefinition>() },
          { typeof(EffectOverride), new AssetBundleLoader<EffectOverride>() },
          { typeof(PlanetNameOverride), new AssetBundleLoader<PlanetNameOverride>() },
          { typeof(ModdedWeathersMatcher), new AssetBundleLoader<ModdedWeathersMatcher>() },
        }
      );
    }

    public override void ConvertLoadedAssets()
    {
      foreach (Editor.WeatherDefinition WeatherDefinition in GetLoadedAssets<Editor.WeatherDefinition>())
      {
        Logger.LogWarning($"Registering weather definition from asset bundle: {WeatherDefinition.Name}");

        NewerWeatherDefinition ConvertedDefinition = ScriptableObject.CreateInstance<NewerWeatherDefinition>();
        ConvertedDefinition.Name = WeatherDefinition.Name;
        ConvertedDefinition.Color = ColorHelper.ToTMPColorGradient(WeatherDefinition.Color);
        ConvertedDefinition.Effect = WeatherDefinition.Effect;
        ConvertedDefinition.Config = WeatherDefinition.Config;

        WeatherManager.RegisterWeather(ConvertedDefinition);
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

        ImprovedWeather weather = ConfigHelper.ResolveStringToWeather(effectOverride.weatherName);
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
            WeatherOverrideManager.PlanetOverrideNames.Add(newOverride, planetNameOverride.newPlanetName);
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
              ImprovedWeather matchedWeather = ConfigHelper.ResolveStringToWeather(weather.Name);
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
