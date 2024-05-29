using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace WeatherAPI
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

    public override void OnNetworkSpawn()
    {
      base.OnNetworkSpawn();
      gameObject.name = "WeatherSync";
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }

    private string LatestWeathersReceived = "";

    // this whole stuff is not working at all (yet)

    [ServerRpc]
    public void SendWeathersRequest()
    {
      Plugin.logger.LogInfo("Received weather request, sending");
      Dictionary<string, LevelWeatherType> weathers = WeatherCalculation.NewWeathers(StartOfRound.Instance);
      SendWeathers(weathers);
    }

    [ServerRpc]
    public void SendWeathers(Dictionary<string, LevelWeatherType> weathers)
    {
      string serialized = JsonConvert.SerializeObject(weathers);
      Plugin.logger.LogInfo($"Sending weathers: {serialized}");

      ApplyWeathers(serialized);
    }

    [ClientRpc]
    public void ApplyWeathers(string weathers)
    {
      Plugin.logger.LogInfo($"Received weathers: {weathers}");

      if (LatestWeathersReceived == weathers)
      {
        Plugin.logger.LogInfo("Weathers are the same as last ones, skipping");
        return;
      }

      Dictionary<string, LevelWeatherType> newWeathers = JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(weathers);

      foreach (SelectableLevel level in StartOfRound.Instance.levels)
      {
        level.currentWeather = newWeathers[level.PlanetName];
      }

      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    // this stuff above isn't working as i want
    // i want to have:
    // 1. a function that sends weathers to all clients (only from host)
    // 2. a function that receives weathers
    // 3. a function that applies weathers to levels
    // 4. a function that can call server that it should send weathers
  }
}
