using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace WeatherAPI.Patches
{
  [HarmonyPatch(typeof(GameNetworkManager), "Start")]
  class GameNetworkManagerStartPatch
  {
    [HarmonyPostfix]
    public static void GameMethodPatch(GameNetworkManager __instance)
    {
      var prefab = new GameObject("WeatherAPISyncInit");
      prefab.hideFlags = HideFlags.HideAndDontSave;
      prefab.AddComponent<WeatherSync>();
      prefab.AddComponent<NetworkObject>();

      GameObject.DontDestroyOnLoad(prefab);

      WeatherSync.networkManager = __instance.GetComponent<NetworkManager>();
      WeatherSync.WeatherSyncPrefab = prefab;
      // WeatherSync.Instance = prefab.GetComponent<WeatherSync>();

      // GameObject.Instantiate(prefab);

      Plugin.logger.LogWarning("WeatherSync initialized in GameNetworkManager.Start");
    }
  }
}
