using System.Collections.Generic;
using HarmonyLib;
using LethalLib.Modules;
using Newtonsoft.Json;
using Unity.Netcode;

namespace WeatherAPI.Patches
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

      if (__instance == null)
      {
        Plugin.logger.LogWarning("Instance is null");
        return true;
      }

      if (__instance.IsHost)
      {
        WeatherManager.CurrentWeathers = [];

        Dictionary<string, LevelWeatherType> newWeathers = WeatherCalculation.NewWeathers(__instance);

        new WeatherSync().SendWeathers(newWeathers);
      }

      return false;
    }
  }
}
