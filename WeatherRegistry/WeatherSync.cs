using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WeatherRegistry
{
  internal class WeatherSync : NetworkBehaviour
  {
    public static GameObject WeatherSyncPrefab;
    private static WeatherSync _instance;
    public static WeatherSync Instance
    {
      get
      {
        if (_instance == null)
          _instance = UnityEngine.Object.FindObjectOfType<WeatherSync>();
        if (_instance == null)
          Plugin.logger.LogError("WeatherSync instance is null");
        return _instance;
      }
      set { _instance = value; }
    }
    public static NetworkManager networkManager;

    private static List<GameObject> queuedNetworkPrefabs = [];
    public static bool networkHasStarted = false;

    public override void OnNetworkSpawn()
    {
      base.OnNetworkSpawn();
      gameObject.name = "WeatherSync";
      Instance = this;
      DontDestroyOnLoad(gameObject);

      Plugin.logger.LogWarning($"WeathersSynced: {WeathersSynced.Value}");

      WeathersSynced.OnValueChanged += WeathersReceived;
    }

    private string LatestWeathersReceived = "";
    private static readonly string DefaultValue = "{}";

    public NetworkVariable<FixedString4096Bytes> WeathersSynced = new(DefaultValue);
    public string Weather
    {
      get => WeathersSynced.Value.ToString();
      set => WeathersSynced.Value = new FixedString4096Bytes(value);
    }

    public void SetNew(string weathers)
    {
      Plugin.logger.LogInfo($"Setting new weathers: {weathers}");
      Plugin.logger.LogInfo($"Current weathers: {Weather} (is null? {Weather == null}) (is empty? {Weather == ""}");
      Weather = weathers;
    }

    // this whole stuff is not working at all (yet)

    public void WeathersReceived(FixedString4096Bytes oldWeathers, FixedString4096Bytes weathers)
    {
      Plugin.logger.LogInfo($"Weathers received: {weathers}");

      if (!WeatherManager.IsSetupFinished)
      {
        return;
      }

      ApplyWeathers(weathers.ToString());
    }

    public void ApplyWeathers(string weathers)
    {
      Plugin.logger.LogInfo($"Weathers to apply: {weathers}");

      if (LatestWeathersReceived == weathers)
      {
        Plugin.logger.LogInfo("Weathers are the same as last ones, skipping");
        return;
      }

      if (weathers == DefaultValue)
      {
        Plugin.logger.LogInfo("Weathers are not set, skipping");
        return;
      }

      Dictionary<string, LevelWeatherType> newWeathers = JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(weathers);

      foreach (SelectableLevel level in StartOfRound.Instance.levels)
      {
        level.currentWeather = newWeathers[level.PlanetName];
      }

      LatestWeathersReceived = weathers;
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
      if (networkHasStarted == false)
      {
        Plugin.logger.LogWarning("Registering NetworkPrefab: " + prefab);
        queuedNetworkPrefabs.Add(prefab);
      }
      else
      {
        Plugin.logger.LogWarning("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!");
      }
    }

    internal static void RegisterPrefabs(NetworkManager networkManager)
    {
      Plugin.logger.LogWarning("Registering NetworkPrefabs in NetworkManager");

      List<GameObject> addedNetworkPrefabs = new List<GameObject>();

      foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
      {
        addedNetworkPrefabs.Add(networkPrefab.Prefab);
      }

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

      networkHasStarted = true;
    }
  }
}
