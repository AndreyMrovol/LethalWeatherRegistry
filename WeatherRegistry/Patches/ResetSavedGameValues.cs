using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(GameNetworkManager))]
  class ResetLobbyPatch
  {
    [HarmonyPatch("ResetSavedGameValues")]
    [HarmonyPrefix]
    public static void ResetSavedGameValuesPatch()
    {
      // Remove all save entries made by Registry
      Plugin.logger.LogDebug("Removing all save entries made by WeatherRegistry");
      WeatherManager.CurrentWeathers.Clear();

      for (int i = 0; i <= 100; i++)
      {
        string SaveKey = $"{Defaults.WeatherSaveKey}-{i}";

        if (ES3.KeyExists(SaveKey, GameNetworkManager.Instance.currentSaveFileName))
        {
          ES3.DeleteKey(SaveKey, GameNetworkManager.Instance.currentSaveFileName);
        }
      }
    }
  }
}
