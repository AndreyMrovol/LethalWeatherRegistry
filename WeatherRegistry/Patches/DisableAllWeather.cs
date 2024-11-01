using System;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static partial class TimeOfDayPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherRegistry TimeOfDay");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "DisableAllWeather")]
    private static void DisableAllWeatherPatch(TimeOfDay __instance, bool deactivateObjects)
    {
      logger.LogDebug("Disabling all weather");

      if (!deactivateObjects)
      {
        return;
      }

      logger.LogDebug("DecativateObjects is true");

      foreach (RegistryWeatherEffect effect in WeatherManager.Weathers.Select(weather => weather.Effect))
      {
        effect.DisableEffect(deactivateObjects);
      }

      EventManager.DisableAllWeathers.Invoke();

      WeatherRegistry.Patches.SunAnimator.Clear();
    }
  }
}
