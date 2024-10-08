using System.Collections.Generic;
using HarmonyLib;
using LethalLib.Modules;
using Newtonsoft.Json;
using Unity.Netcode;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  class SetPlanetsWeatherPatch
  {
    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPrefix]
    public static bool GameMethodPatch(int connectedPlayersOnServer, StartOfRound __instance)
    {
      Plugin.logger.LogInfo("SetPlanetsWeather called.");

      if (!WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("WeatherManager is not set up yet.");
        return false;
      }

      if (!Settings.SelectWeathers)
      {
        Plugin.logger.LogDebug("Weather selection is disabled.");
        return true;
      }

      if (__instance == null)
      {
        Plugin.logger.LogWarning("Instance is null");
        return true;
      }

      if (__instance.IsHost)
      {
        // WeatherManager.CurrentWeathers = [];

        Dictionary<SelectableLevel, LevelWeatherType> newWeathers = WeatherCalculation.weatherSelectionAlgorithm.SelectWeathers(
          connectedPlayersOnServer,
          __instance
        );

        Plugin.logger.LogDebug($"Instance: {WeatherSync.Instance}");
        Plugin.logger.LogDebug($"Weathers: {newWeathers}");
        Plugin.logger.LogDebug($"WeatherSync: {WeatherSync.Instance.WeathersSynced}");
        Plugin.logger.LogDebug($"WeathersSynced: {WeatherSync.Instance.WeathersSynced.Value}");

        WeatherManager.currentWeathers.Entries = newWeathers;
        WeatherSync.Instance.SetNewOnHost(WeatherManager.currentWeathers.SerializedEntries);
      }

      EventManager.DayChanged.Invoke(__instance.gameStats.daysSpent);

      return false;
    }
  }
}
