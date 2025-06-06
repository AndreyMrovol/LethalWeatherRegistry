using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using WeatherRegistry;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(EntranceTeleport))]
  internal class EntranceTeleportPatch
  {
    internal static MrovLib.Logger logger = new("EntranceTeleport");

    [Obsolete("Use Settings.IsPlayerInside instead")]
    internal static bool isPlayerInside => Settings.IsPlayerInside;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayer")]
    private static void TeleportPlayerPatch(EntranceTeleport __instance)
    {
      logger.LogDebug($"TeleportPlayerPatch called with {__instance.name}");

      Settings.IsPlayerInside = __instance.isEntranceToBuilding;

      if (Settings.IsPlayerInside)
      {
        logger.LogDebug("Player is inside");
      }
      else
      {
        logger.LogDebug("Player is outside");

        WeatherEffectController.EnableCurrentWeatherEffects();
      }
    }
  }
}
