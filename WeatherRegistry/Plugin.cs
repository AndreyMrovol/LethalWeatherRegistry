using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using WeatherRegistry.Compatibility;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  [BepInPlugin(GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("MrovLib", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("mattymatty.LobbyControl", BepInDependency.DependencyFlags.SoftDependency)]
  public class Plugin : BaseUnityPlugin
  {
    public const string GUID = "mrov.WeatherRegistry";

    internal static ManualLogSource logger;
    internal static MrovLib.Logger debugLogger = new(GUID);
    internal static Harmony harmony = new(Plugin.GUID);

    internal static bool IsLethalLibLoaded = false;
    internal static JLLCompat JLLCompat;
    internal static LobbyControlCompat LobbyControlCompat;
    internal static FacilityMeltdownCompat FacilityMeltdownCompat;
    internal static OrbitsCompat OrbitsCompat;

    internal static Hook WeatherTypeEnumHook;

    private void Awake()
    {
      logger = Logger;
      harmony.PatchAll();

      ConfigManager.Init(Config);
      SunAnimator.Init();

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

      if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
      {
        LobbyCompatibilityCompatibility.Init();
      }

      JLLCompat = new JLLCompat("JacobG5.JLL");
      LobbyControlCompat = new LobbyControlCompat("mattymatty.LobbyControl");
      LobbyControlCompat.Init();
      FacilityMeltdownCompat = new FacilityMeltdownCompat("me.loaforc.facilitymeltdown");
      OrbitsCompat = new OrbitsCompat("com.fiufki.orbits");

      // Plugin startup logic
      Logger.LogInfo($"Plugin {Plugin.GUID} is loaded!");
    }
  }
}
