using System;
using HarmonyLib;
using UnityEngine;

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

        Plugin.debugLogger.LogInfo($"ApparatusSpawnAfter: {__instance.scrapValue}");
      }
      catch (Exception exception)
      {
        Plugin.logger.LogError(exception.Message);
        return true;
      }

      return true;
    }
  }
}
