using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(RoundManager))]
  public class SpawnScrapInLevelPatches
  {
    [HarmonyPatch("SpawnScrapInLevel")]
    [HarmonyAfter(["com.github.fredolx.meteomultiplier", "DarthLilo.WeatherBonuses"])]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    private static void ChangeMultipliers(RoundManager __instance)
    {
      if (!ConfigManager.UseScrapMultipliers.Value)
      {
        Plugin.logger.LogDebug("Skipped using WeatherRegistry's scrap multipliers.");
        return;
      }

      Weather currentWeather = WeatherManager.GetCurrentWeather(__instance.currentLevel);

      // why would the default vanilla value be 0.4? no fucking clue
      // but a layer of abstraction is very much welcome
      __instance.scrapValueMultiplier = currentWeather.ScrapValueMultiplier * 0.4f;
      __instance.scrapAmountMultiplier = currentWeather.ScrapAmountMultiplier;
    }

    [HarmonyPatch("SpawnScrapInLevel")]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    private static void LogMultipliers(RoundManager __instance)
    {
      Plugin.logger.LogInfo(
        $"Spawned scrap in level with multipliers: {__instance.scrapValueMultiplier} (value), {__instance.scrapAmountMultiplier} (amount)"
      );
    }
  }
}
