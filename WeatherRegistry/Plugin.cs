﻿using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Compatibility;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("MrovLib", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("mattymatty.LobbyControl", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInIncompatibility("Ozzymops.DisableStormyWeather")]
  public class Plugin : BaseUnityPlugin
  {
    [Obsolete("Use PluginInfo.PLUGIN_GUID instead")]
    public const string GUID = PluginInfo.PLUGIN_GUID;

    internal static ManualLogSource logger;
    internal static Logger debugLogger = new("Debug", LoggingType.Debug);
    internal static Harmony harmony = new(PluginInfo.PLUGIN_GUID);

    internal static bool IsLethalLibLoaded = false;
    internal static JLLCompat JLLCompat;
    internal static LobbyControlCompat LobbyControlCompat;
    internal static FacilityMeltdownCompat FacilityMeltdownCompat;
    internal static OrbitsCompat OrbitsCompat;
    internal static ImperiumCompat ImperiumCompat;
    internal static MalfunctionsCompat MalfunctionsCompat;

    internal static Hook WeatherTypeEnumHook;

    internal static TerminalKeyword ForecastVerb;

    private void Awake()
    {
      logger = Logger;
      harmony.PatchAll();

      ConfigManager.Init(Config);

#if DEVMODE
      Plugin.logger.LogWarning("Dev build detected, enabling full debug logging.");
      ConfigManager.LoggingLevels.Value = LoggingType.Developer;
#endif

      AssetBundleLoader.LoadAssetBundles();

      ForecastVerb = ScriptableObject.CreateInstance<TerminalKeyword>();
      ForecastVerb.name = "Forecast";
      ForecastVerb.word = "forecast";
      ForecastVerb.isVerb = true;

      MrovLib.EventManager.MainMenuLoaded.AddListener(() =>
      {
        MainMenuInit();
      });

      EventManager.SetupFinished.AddListener(() =>
      {
        ContentManager.AddTerminalKeywords([ForecastVerb]);

        var (compatibleNouns, forecastNodes, forecastKeywords) = Forecasts.InitializeForecastNodes();

        ForecastVerb.compatibleNouns = compatibleNouns.ToArray();
        ContentManager.AddTerminalNodes(forecastNodes);
        ContentManager.AddTerminalKeywords(forecastKeywords);

        ConfigManager.Instance.RemoveOrphanedEntries();
      });

      EventManager.SetupFinished.AddListener(() =>
      {
        AssetBundleLoader.LoadWeatherOverrides();
        AssetBundleLoader.LoadModdedWeathersMatchers();
      });

      if (Chainloader.PluginInfos.ContainsKey("evaisa.lethallib"))
      {
        IsLethalLibLoaded = true;
        LethalLibPatch.Init();
      }
      else
      {
        logger.LogDebug("LethalLib not detected!");
      }

      WeatherTypeEnumHook = new Hook(
        typeof(Enum).GetMethod("ToString", []),
        typeof(EnumPatches).GetMethod(nameof(EnumPatches.LevelWeatherTypeEnumToStringHook))
      );

      if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
      {
        LobbyCompatibilityCompatibility.Init();
      }

      JLLCompat = new JLLCompat("JacobG5.JLL");

      LobbyControlCompat = new LobbyControlCompat("mattymatty.LobbyControl");
      LobbyControlCompat.Init();

      FacilityMeltdownCompat = new FacilityMeltdownCompat("me.loaforc.facilitymeltdown");

      OrbitsCompat = new OrbitsCompat("com.fiufki.orbits");
      OrbitsCompat.Init();

      ImperiumCompat = new ImperiumCompat("giosuel.Imperium");

      MalfunctionsCompat = new MalfunctionsCompat("com.zealsprince.malfunctions");

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void MainMenuInit()
    {
      ImperiumCompat.Init();
    }
  }
}
