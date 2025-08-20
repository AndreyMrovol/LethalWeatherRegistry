using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Editor;

namespace WeatherRegistry
{
  public class AssetBundleLoader
  {
    internal static DirectoryInfo pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;
    private static Dictionary<string, AssetBundle> LoadedBundles = [];

    private static List<EffectOverride> LoadedEffectOverrides = [];
    private static List<PlanetNameOverride> LoadedPlanetNameOverrides = [];

    private static readonly Logger Logger = new("AssetBundleLoader", LoggingType.Debug);

    internal static void LoadAssetBundles()
    {
      // Path where you'll place your asset bundles (next to the plugin DLL)
      string bundlesPath = pluginsFolder.FullName;

      if (!Directory.Exists(bundlesPath))
      {
        Logger.LogWarning($"AssetBundles folder not found: {bundlesPath}");
        return;
      }

      // Load specific bundle by name (if you know the name)
      // LoadSpecificBundle(bundlesPath, "Weather");

      // Or load all bundles in the folder
      LoadAllBundlesInFolder(bundlesPath);

      Logger.LogCustom(
        $"Loaded {WeatherManager.RegisteredWeathers.Count} weather definitions from asset bundles: [{string.Join(", ", WeatherManager.RegisteredWeathers.Select(w => w.Name))}]",
        LogLevel.Info,
        LoggingType.Basic
      );
    }

    private static void LoadAllBundlesInFolder(string bundlesPath)
    {
      string[] bundleFiles = Directory.GetFiles(bundlesPath, $"*.weatherbundle", SearchOption.AllDirectories);

      foreach (string bundlePath in bundleFiles)
      {
        string fileName = Path.GetFileName(bundlePath);
        LoadBundle(bundlePath, fileName);
      }
    }

    private static void LoadBundle(string bundlePath, string bundleName)
    {
      try
      {
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

        if (bundle == null)
        {
          Logger.LogError($"Failed to load asset bundle: {bundleName}");
          return;
        }

        LoadedBundles[bundleName] = bundle;
        Logger.LogInfo($"Loaded asset bundle: {bundleName}");

        // Load all Weather from this bundle
        LoadWeatherFromBundle(bundle, bundleName);
      }
      catch (System.Exception e)
      {
        Logger.LogError($"Error loading bundle {bundleName}: {e.Message}");
      }
    }

    private static void LoadWeatherFromBundle(AssetBundle bundle, string bundleName)
    {
      // Get all asset names in the bundle (optional - for debugging)
      string[] assetNames = bundle.GetAllAssetNames();
      Logger.LogInfo($"Bundle {bundleName} contains {assetNames.Length} assets");

      // Load all WeatherDefinition assets
      WeatherDefinition[] WeatherDefinitionAssets = bundle.LoadAllAssets<WeatherDefinition>();
      LoadedEffectOverrides.AddRange(bundle.LoadAllAssets<EffectOverride>().ToList());
      LoadedPlanetNameOverrides.AddRange(bundle.LoadAllAssets<PlanetNameOverride>().ToList());

      foreach (WeatherDefinition WeatherDefinition in WeatherDefinitionAssets)
      {
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

        Weather weather =
          new(WeatherDefinition.Name, newImprovedWeatherEffect)
          {
            Color = WeatherDefinition.Color,
            Origin = WeatherOrigin.WeatherRegistry,
            Type = WeatherType.Modded,
            Config = WeatherDefinition.Config.CreateFullConfig(),
            hideFlags = HideFlags.HideAndDontSave
          };

        GameObject.DontDestroyOnLoad(weather);
        WeatherManager.RegisterWeather(weather);
      }
    }

    public static void LoadWeatherOverrides()
    {
      // Load all EffectOverride assets
      foreach (EffectOverride effectOverride in LoadedEffectOverrides)
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
            WeatherOverrideManager.PlanetOverrideNames.Add(newOverride, planetNameOverride.newPlanetName);
          }
        }
      }
    }
  }
}
