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
      if (__instance.IsHost)
      {
        Weather weather = WeatherManager.GetCurrentLevelWeather();

        __instance.SetScrapValue((int)(__instance.scrapValue * weather.ScrapValueMultiplier));
      }
    }
  }
}
