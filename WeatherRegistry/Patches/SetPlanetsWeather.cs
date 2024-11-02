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
        string SaveKey = $"{Defaults.WeatherSaveKey}-{StartOfRound.Instance.gameStats.daysSpent}";
        bool weathersAlreadySelected = ES3.KeyExists(SaveKey, GameNetworkManager.Instance.currentSaveFileName);

        if (weathersAlreadySelected)
        {
          Plugin.logger.LogInfo(
            $"Loading picked weathers from save: day {StartOfRound.Instance.gameStats.daysSpent}, file {GameNetworkManager.Instance.currentSaveFileName}"
          );

          Plugin.logger.LogDebug($"Weathers: {ES3.Load<string>(SaveKey, GameNetworkManager.Instance.currentSaveFileName)}");

          WeatherManager.currentWeathers.SetWeathersFromStringDictionary(
            ES3.Load<string>(SaveKey, GameNetworkManager.Instance.currentSaveFileName)
          );
        }
        else
        {
          Dictionary<SelectableLevel, LevelWeatherType> newWeathers = WeatherCalculation.weatherSelectionAlgorithm.SelectWeathers(
            connectedPlayersOnServer,
            __instance
          );

          WeatherManager.currentWeathers.SetWeathers(newWeathers);
        }

        ES3.Save<string>(SaveKey, WeatherManager.currentWeathers.SerializedEntries, GameNetworkManager.Instance.currentSaveFileName);
        Plugin.logger.LogDebug(
          $"Saved picked weathers: day {StartOfRound.Instance.gameStats.daysSpent}, file {GameNetworkManager.Instance.currentSaveFileName}"
        );
      }

      EventManager.DayChanged.Invoke(__instance.gameStats.daysSpent);

      return false;
    }
  }
}
