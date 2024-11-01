using HarmonyLib;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  class StartOfRoundPatch
  {
    [HarmonyPatch("OnDisable")]
    [HarmonyPrefix]
    public static void DisableWeathersPatch()
    {
      foreach (RegistryWeather weather in WeatherManager.Weathers)
      {
        weather.Effect.DisableEffect(true);
      }

      EventManager.DisableAllWeathers.Invoke();
    }
  }
}
