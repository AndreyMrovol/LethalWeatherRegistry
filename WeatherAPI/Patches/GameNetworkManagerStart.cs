using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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

      var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + "weathersync"));
      prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);

      // GameObject.Instantiate(prefab);

      WeatherSync.networkManager.AddNetworkPrefab(prefab);

      Plugin.logger.LogWarning("WeatherSync initialized in GameNetworkManager.Start");
    }
  }
}
