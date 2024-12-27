using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(LungProp))]
  static class LungPropPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch("DisconnectFromMachinery")]
    public static void DisconnectFromMachineryPatch(LungProp __instance)
    {
      if (Plugin.FacilityMeltdownCompat.IsModPresent)
      {
        Plugin.logger.LogInfo("FacilityMeltdown is present - WeatherRegistry will not run its apparatus patch.");
        return;
      }

      if (__instance.IsHost)
      {
        Weather weather = WeatherManager.GetCurrentLevelWeather();

        __instance.SetScrapValue((int)(__instance.scrapValue * weather.ScrapValueMultiplier));
      }
    }
  }
}
