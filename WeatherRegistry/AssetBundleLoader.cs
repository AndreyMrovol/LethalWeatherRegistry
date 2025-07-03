using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
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

      foreach (WeatherDefinition WeatherDefinition in WeatherDefinitionAssets)
      {
        Weather weather = ScriptableObject.CreateInstance<Weather>();
        weather.name = WeatherDefinition.Name;
        weather.Color = WeatherDefinition.Color;
        weather.Effect = GameObject.Instantiate(WeatherDefinition.Effect);
        weather.Origin = WeatherOrigin.WeatherRegistry;
        weather.Type = WeatherType.Modded;
        weather.Config = WeatherDefinition.Config.CreateFullConfig();

        weather.hideFlags = HideFlags.HideAndDontSave;

        weather.Effect.EffectObject = GameObject.Instantiate(weather.Effect.EffectObject);
        weather.Effect.WorldObject = GameObject.Instantiate(weather.Effect.WorldObject);

        LoadedWeather.Add(weather);
      }

      // TODO: remove this when out of dev phase

      // Load all Weather assets
      Weather[] WeatherAssets = bundle.LoadAllAssets<Weather>();

      foreach (Weather Weather in WeatherAssets)
      {
        Plugin.debugLogger.LogError($"Weather {Weather.name} is using OldWeatherDefinition!");
        Plugin.debugLogger.LogWarning("Please update it to use WeatherDefinition.");

        Weather.hideFlags = HideFlags.HideAndDontSave;
        Weather.Origin = WeatherOrigin.WeatherRegistry;
        Weather.Config = new() { };
        Weather.WeatherVariables = [];
        Weather.WeatherEffectOverrides = [];

        GameObject.Instantiate(Weather);
        GameObject.Instantiate(Weather.Effect);

        // Weather.Effect.EffectObject = GameObject.Instantiate(Weather.Effect.EffectObject);
        // Weather.Effect.WorldObject = GameObject.Instantiate(Weather.Effect.WorldObject);

        LoadedWeather.Add(Weather);
      }
    }
  }
}
