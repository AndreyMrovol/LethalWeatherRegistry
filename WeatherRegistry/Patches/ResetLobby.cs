namespace WeatherRegistry.Patches
{
  using System.Collections.Generic;
  using System.Linq;
  using HarmonyLib;
  using UnityEngine;

  [HarmonyPatch(typeof(GameNetworkManager))]
  class ResetLobbyPatch
  {
    [HarmonyPatch("ResetSavedGameValues")]
    [HarmonyPrefix]
    public static void ResetSavedGameValuesPatch()
    {
      WeatherEffect[] effects = TimeOfDay.Instance.effects;
      List<WeatherEffect> effectList = effects.ToList();

      // Remove all weather effects past the vanilla indexes
      int lowestModdedIndex = WeatherManager.ModdedWeatherEnumExtension.Keys.Min();
      Plugin.debugLogger.LogInfo($"Lowest modded index: {lowestModdedIndex}");
      for (int i = lowestModdedIndex; i < effects.Length; i++)
      {
        WeatherEffect effect = effects[i];
        if (effect != null)
        {
          effectList.Remove(effect);
        }
      }

      TimeOfDay.Instance.effects = effectList.ToArray();
    }
  }
}
