using System;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(LungProp))]
  static class LungPropPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch("Start")]
    public static bool StartPatch(LungProp __instance)
    {
      if (Plugin.FacilityMeltdownCompat.IsModPresent)
      {
        Plugin.logger.LogInfo("FacilityMeltdown is present - WeatherRegistry will not run its apparatus patch.");
        return true;
      }

      if (!Settings.ScrapMultipliers)
      {
        Plugin.logger.LogInfo("Skipped using WeatherRegistry's scrap multipliers.");
        return true;
      }

      UpdateScanNode(__instance);

      if (!Settings.IsGameStarted)
      {
        Plugin.logger.LogInfo("Game has not been started yet, skipping WeatherRegistry's apparatus patch.");
        return true;
      }

      try
      {
        Plugin.debugLogger.LogInfo($"ApparatusSpawnBefore: {__instance.scrapValue}");

        Weather weather = WeatherManager.GetCurrentLevelWeather();
        Plugin.debugLogger.LogInfo($"Scrap multiplier: {weather.ScrapValueMultiplier}");
        __instance.SetScrapValue((int)(__instance.scrapValue * weather.ScrapValueMultiplier));

        // change the tooltip value of apparatus to ???
        UpdateScanNode(__instance);

        Plugin.debugLogger.LogInfo($"ApparatusSpawnAfter: {__instance.scrapValue}");
      }
      catch (Exception exception)
      {
        Plugin.logger.LogError(exception.Message);
        return true;
      }

      return true;
    }

    private static void UpdateScanNode(LungProp lungProp)
    {
      var scanNode = lungProp.gameObject.GetComponentInChildren<ScanNodeProperties>();
      if (scanNode != null)
      {
        scanNode.subText = "Value: $???";
      }
    }
  }
}
