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
      if (!Settings.ScrapMultipliers)
      {
        Plugin.logger.LogInfo("Skipped using WeatherRegistry's scrap multipliers.");
        return;
      }

      if (Plugin.BrutalCompanyCompat.IsModPresent)
      {
        Plugin.logger.LogInfo(
          $"Brutal Company detected, adding BC-ER scrap multipliers {Plugin.BrutalCompanyCompat.GetScrapValueMultiplier()} (value); {Plugin.BrutalCompanyCompat.GetScrapAmountMultiplier()} (amount)"
        );
        __instance.scrapValueMultiplier = Plugin.BrutalCompanyCompat.GetScrapValueMultiplier();
        __instance.scrapAmountMultiplier = Plugin.BrutalCompanyCompat.GetScrapAmountMultiplier();
      }

      Settings.IsGameStarted = true;

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
        $"Spawned scrap in level with multipliers: {__instance.scrapValueMultiplier} (value); {__instance.scrapAmountMultiplier} (amount)"
      );
    }
  }
}
