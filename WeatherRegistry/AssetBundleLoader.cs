using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Editor;

namespace WeatherRegistry
{
  public class AssetBundleLoader
  {
    internal static DirectoryInfo pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;
    public static List<Weather> LoadedWeather { get; private set; } = [];
    private static Dictionary<string, AssetBundle> LoadedBundles = [];

    internal static void LoadAssetBundles()
    {
      // Path where you'll place your asset bundles (next to the plugin DLL)
      string bundlesPath = pluginsFolder.FullName;

      if (!Directory.Exists(bundlesPath))
      {
        Plugin.debugLogger.LogWarning($"AssetBundles folder not found: {bundlesPath}");
        return;
      }

      // Load specific bundle by name (if you know the name)
      // LoadSpecificBundle(bundlesPath, "Weather");

      // Or load all bundles in the folder
      LoadAllBundlesInFolder(bundlesPath);

      Plugin.logger.LogInfo(
        $"Loaded {LoadedWeather.Count} weather definitions from asset bundles: {string.Join(", ", LoadedWeather.Select(w => (w.name, (int)w.VanillaWeatherType)))}"
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
          Plugin.debugLogger.LogError($"Failed to load asset bundle: {bundleName}");
          return;
        }

        LoadedBundles[bundleName] = bundle;
        Plugin.debugLogger.LogInfo($"Loaded asset bundle: {bundleName}");

        // Load all Weather from this bundle
        LoadWeatherFromBundle(bundle, bundleName);
      }
      catch (System.Exception e)
      {
        Plugin.debugLogger.LogError($"Error loading bundle {bundleName}: {e.Message}");
      }
    }

    private static void LoadWeatherFromBundle(AssetBundle bundle, string bundleName)
    {
      // Get all asset names in the bundle (optional - for debugging)
      string[] assetNames = bundle.GetAllAssetNames();
      Plugin.debugLogger.LogInfo($"Bundle {bundleName} contains {assetNames.Length} assets");

      // Load all WeatherDefinition assets
      WeatherDefinition[] WeatherDefinitionAssets = bundle.LoadAllAssets<WeatherDefinition>();
      EffectOverride[] effectOverrides = bundle.LoadAllAssets<EffectOverride>();

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

      // Load all EffectOverride assets
      foreach (EffectOverride effectOverride in effectOverrides)
      {
        if (effectOverride == null || string.IsNullOrEmpty(effectOverride.weatherName))
        {
          Plugin.debugLogger.LogWarning("EffectOverride is null or has no weatherName, skipping.");
          continue;
        }

        Weather weather = ConfigHelper.ResolveStringToWeather(effectOverride.weatherName);
        if (weather == null)
        {
          Plugin.debugLogger.LogWarning($"Weather {effectOverride.weatherName} not found, skipping EffectOverride.");
          continue;
        }

        SelectableLevel level = ConfigHelper.ConvertStringToLevels(effectOverride.levelName).FirstOrDefault();

        ImprovedWeatherEffect overrideEffect = effectOverride.OverrideEffect;

        WeatherEffectOverride newOverride =
          new(weather, level, overrideEffect, effectOverride.weatherDisplayName, effectOverride.weatherDisplayColor);
      }
    }
  }
}
