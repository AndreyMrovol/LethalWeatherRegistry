using HarmonyLib;

namespace WeatherRegistry.Patches
{
  class StartOfRoundPatch
  {
    [HarmonyPatch("OnDisable")]
    [HarmonyPrefix]
    public static void DisableWeathersPatch()
    {
      foreach (Weather weather in WeatherManager.Weathers)
      {
        weather.Effect.DisableEffect();
      }
    }
  }
}
