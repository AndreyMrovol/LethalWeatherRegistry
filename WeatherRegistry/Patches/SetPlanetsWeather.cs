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
      Plugin.logger.LogMessage("SetPlanetsWeather called.");

      if (!WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("WeatherManager is not set up yet.");
        return false;
      }

      if (!Settings.SelectWeathers)
      {
        Plugin.logger.LogInfo("Weather selection is disabled.");
        return true;
      }

      if (__instance == null)
      {
        Plugin.logger.LogWarning("Instance is null");
        return true;
      }

      if (__instance.IsHost)
      {
        WeatherManager.CurrentWeathers = [];

        Dictionary<string, LevelWeatherType> newWeathers = WeatherCalculation.NewWeathers(__instance);

        Plugin.logger.LogDebug($"Instance: {WeatherSync.Instance}");
        Plugin.logger.LogDebug($"Weathers: {newWeathers}");
        Plugin.logger.LogDebug($"WeatherSync: {WeatherSync.Instance.WeathersSynced}");
        Plugin.logger.LogDebug($"WeathersSynced: {WeatherSync.Instance.WeathersSynced.Value}");

        WeatherSync.Instance.SetNew(JsonConvert.SerializeObject(newWeathers));
      }

      return false;
    }
  }
}
