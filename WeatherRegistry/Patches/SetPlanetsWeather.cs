using System.Collections.Generic;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  class SetPlanetsWeatherPatch
  {
    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPrefix]
    public static bool GameMethodPatch(int connectedPlayersOnServer, StartOfRound __instance)
    {
      Plugin.logger.LogDebug("SetPlanetsWeather called.");

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

      Plugin.logger.LogInfo(
        $"Picking weathers on day {StartOfRound.Instance.gameStats.daysSpent}, file {GameNetworkManager.Instance.currentSaveFileName}"
      );

      if (__instance.IsHost)
      {
        string SaveKey = $"{Defaults.WeatherSaveKey}-{StartOfRound.Instance.gameStats.daysSpent}";
        bool weathersAlreadySelected = ES3.KeyExists(SaveKey, GameNetworkManager.Instance.currentSaveFileName);

        Plugin.logger.LogInfo($"Save file has weather data: {weathersAlreadySelected}");

        if (weathersAlreadySelected)
        {
          Plugin.logger.LogInfo(
            $"Loading picked weathers from save: day {StartOfRound.Instance.gameStats.daysSpent}, file {GameNetworkManager.Instance.currentSaveFileName}"
          );

          Plugin.logger.LogDebug($"Weathers: {ES3.Load<string>(SaveKey, GameNetworkManager.Instance.currentSaveFileName)}");

          WeatherManager.CurrentWeathers.SetWeathersFromStringDictionary(
            ES3.Load<string>(SaveKey, GameNetworkManager.Instance.currentSaveFileName)
          );
        }
        else
        {
          Plugin.debugLogger.LogDebug("Weather selection algorithm: " + WeatherCalculation.WeatherSelectionAlgorithm.GetType().Name);

          Dictionary<SelectableLevel, LevelWeatherType> newWeathers = WeatherCalculation.WeatherSelectionAlgorithm.SelectWeathers(
            connectedPlayersOnServer,
            __instance
          );

          WeatherManager.CurrentWeathers.SetWeathers(newWeathers);
        }

        ES3.Save<string>(SaveKey, WeatherManager.CurrentWeathers.SerializedEntries, GameNetworkManager.Instance.currentSaveFileName);
        Plugin.logger.LogDebug(
          $"Saved picked weathers: day {StartOfRound.Instance.gameStats.daysSpent}, file {GameNetworkManager.Instance.currentSaveFileName}"
        );
      }

      EventManager.DayChanged.Invoke(__instance.gameStats.daysSpent);

      return false;
    }
  }
}
