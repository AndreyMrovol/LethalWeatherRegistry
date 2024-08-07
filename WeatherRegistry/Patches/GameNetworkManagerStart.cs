using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(GameNetworkManager), "Start")]
  class GameNetworkManagerStartPatch
  {
    [HarmonyPrefix]
    public static void GameMethodPatch(GameNetworkManager __instance)
    {
      WeatherSync.networkManager = __instance.GetComponent<NetworkManager>();

      var prefab = new GameObject("WeatherRegistrySyncInit");
      prefab.hideFlags = HideFlags.HideAndDontSave;

      prefab.AddComponent<NetworkObject>();
      var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("weatherregistryweathersync"));
      prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);

      prefab.AddComponent<WeatherSync>();
      prefab.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
      prefab.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
      prefab.GetComponent<NetworkObject>().DestroyWithScene = false;
      GameObject.DontDestroyOnLoad(prefab);

      WeatherSync.WeatherSyncPrefab = prefab;
      WeatherSync.RegisterNetworkPrefab(prefab);

      WeatherSync.RegisterPrefabs(__instance.GetComponent<NetworkManager>());

      Plugin.logger.LogDebug("WeatherSync initialized in GameNetworkManager.Start");
    }
  }
}
