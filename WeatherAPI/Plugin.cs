using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using WeatherAPI.Patches;

namespace WeatherAPI
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
  public class Plugin : BaseUnityPlugin
  {
    internal static ManualLogSource logger;
    internal static Harmony harmony = new(PluginInfo.PLUGIN_GUID);

    internal static bool IsLethalLibLoaded = false;

    internal static Hook WeatherTypeEnumHook;

    private void Awake()
    {
      logger = Logger;
      harmony.PatchAll();

      if (Chainloader.PluginInfos.ContainsKey("evaisa.lethallib"))
      {
        IsLethalLibLoaded = true;
        LethalLibPatch.Init();
      }
      else
      {
        logger.LogInfo("LethalLib not detected!");
      }

      WeatherTypeEnumHook = new Hook(
        typeof(Enum).GetMethod("ToString", []),
        typeof(WeatherManager).GetMethod(nameof(WeatherManager.LevelWeatherTypeEnumHook))
      );

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

  }
}
