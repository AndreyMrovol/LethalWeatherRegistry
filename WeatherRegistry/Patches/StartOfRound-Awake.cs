using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  class StartOfRoundAwakePatch
  {
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    internal static void StartOfRoundAwakePrefix(RoundManager __instance)
    {
      Plugin.debugLogger.LogInfo("StartOfRoundAwakePrefix Patch");

      if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsHost)
      {
        Plugin.debugLogger.LogDebug("Host detected, spawning WeatherSync");
        WeatherSync WeatherSyncPrefab = GameObject.Instantiate(WeatherSync.WeatherSyncPrefab).GetComponent<WeatherSync>();
        WeatherSyncPrefab.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
      }
    }
  }
}
