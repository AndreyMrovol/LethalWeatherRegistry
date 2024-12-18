using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  class StartOfRoundPatch
  {
    [HarmonyPatch("OnDisable")]
    [HarmonyPrefix]
    public static void DisableWeathersPatch()
    {
      foreach (Weather weather in WeatherManager.Weathers)
      {
        weather.Effect.DisableEffect(true);
      }

      HUDManager.Instance.increaseHelmetCondensation = false;
      HUDManager.Instance.HelmetCondensationDrops();

      EventManager.DisableAllWeathers.Invoke();
    }
  }
}
