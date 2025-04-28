using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  internal class WeatherSync : NetworkBehaviour
  {
    public NetworkVariable<WeatherSyncDataWrapper> WeathersSynced = new(new WeatherSyncDataWrapper { Weathers = [] });
    public WeatherSyncData[] Weather
    {
      get => WeathersSynced.Value.Weathers;
      set => WeathersSynced.Value = new WeatherSyncDataWrapper { Weathers = value };
    }

    public NetworkVariable<WeatherEffectDataWrapper> EffectsSynced = new(new WeatherEffectDataWrapper { Effects = [LevelWeatherType.None] });
    public WeatherEffectDataWrapper Effects
    {
      get => EffectsSynced.Value;
      set => EffectsSynced.Value = new WeatherEffectDataWrapper { Effects = value.Effects };
    }

    public NetworkVariable<FixedString4096Bytes> WeatherData = new(new FixedString4096Bytes());
    public FixedString4096Bytes WeatherList
    {
      get => WeatherData.Value;
      set => WeatherData.Value = value;
    }

    public static GameObject WeatherSyncPrefab;
    public static NetworkManager networkManager;
    public static bool networkHasStarted = false;
    private static WeatherSync _instance;
    private static List<GameObject> queuedNetworkPrefabs = [];

    public static WeatherSync Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = UnityEngine.Object.FindObjectOfType<WeatherSync>();
          if (_instance == null)
            Plugin.logger.LogError("WeatherSync instance is null");
        }
        return _instance;
      }
      private set => _instance = value;
    }

    public override void OnNetworkSpawn()
    {
      base.OnNetworkSpawn();
      InitializeInstance();
      WeathersSynced.OnValueChanged += Networking.WeatherLevelData.WeathersReceived;
      EffectsSynced.OnValueChanged += Networking.WeatherEffectData.EffectsReceived;
    }

    public void SetNewOnHost(Dictionary<string, LevelWeatherType> weathers)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        Plugin.logger.LogDebug("Cannot set weathers, not a host!");
        return;
      }

      var weatherData = weathers
        .Select(kvp => new WeatherSyncData { Weather = kvp.Value, LevelName = new FixedString64Bytes(kvp.Key) })
        .ToArray();

      Weather = weatherData;
    }

    public void SetWeatherEffectOnHost(LevelWeatherType weatherType)
    {
      SetWeatherEffectsOnHost([weatherType]);
    }

    public void SetWeatherEffectsOnHost(LevelWeatherType[] weatherTypes)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        Plugin.logger.LogDebug("Cannot set effects, not a host!");
        return;
      }

      Plugin.logger.LogDebug($"Setting effects: [{string.Join("; ", weatherTypes)}]");

      Effects = new WeatherEffectDataWrapper { Effects = weatherTypes };
    }

    #region Prefab registration

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
      if (!networkHasStarted)
      {
        Plugin.logger.LogDebug("Registering NetworkPrefab: " + prefab);
        queuedNetworkPrefabs.Add(prefab);
      }
      else
      {
        Plugin.logger.LogWarning("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!");
      }
    }

    internal static void RegisterPrefabs(NetworkManager networkManager)
    {
      Plugin.logger.LogDebug("Registering NetworkPrefabs in NetworkManager");
      var addedNetworkPrefabs = GetExistingPrefabs(networkManager);
      RegisterQueuedPrefabs(networkManager, addedNetworkPrefabs);
      networkHasStarted = true;
    }

    private void InitializeInstance()
    {
      gameObject.name = "WeatherSync";
      Instance = this;
      DontDestroyOnLoad(gameObject);
      Plugin.logger.LogDebug($"WeathersSynced: {WeathersSynced.Value}");
    }

    private static List<GameObject> GetExistingPrefabs(NetworkManager networkManager)
    {
      var addedNetworkPrefabs = new List<GameObject>();
      foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
      {
        addedNetworkPrefabs.Add(networkPrefab.Prefab);
      }
      return addedNetworkPrefabs;
    }

    private static void RegisterQueuedPrefabs(NetworkManager networkManager, List<GameObject> addedNetworkPrefabs)
    {
      int debugCounter = 0;
      foreach (GameObject queuedNetworkPrefab in queuedNetworkPrefabs)
      {
        Plugin.logger.LogDebug("Trying To Register Prefab: " + queuedNetworkPrefab);
        if (!addedNetworkPrefabs.Contains(queuedNetworkPrefab))
        {
          networkManager.AddNetworkPrefab(queuedNetworkPrefab);
          addedNetworkPrefabs.Add(queuedNetworkPrefab);
        }
        else
          debugCounter++;
      }
      Plugin.logger.LogDebug("Skipped Registering " + debugCounter + " NetworkObjects As They Were Already Registered.");
    }

    #endregion
  }
}
