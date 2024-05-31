using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
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

      Plugin.logger.LogFatal($"WeathersSynced: {WeathersSynced.Value}");

      WeathersSynced.OnValueChanged += WeathersReceived;
    }

    private string LatestWeathersReceived = "";

    public NetworkVariable<FixedString4096Bytes> WeathersSynced = new NetworkVariable<FixedString4096Bytes>("a default value");
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

      Dictionary<string, LevelWeatherType> newWeathers = JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(weathers);

      foreach (SelectableLevel level in StartOfRound.Instance.levels)
      {
        level.currentWeather = newWeathers[level.PlanetName];
      }

      LatestWeathersReceived = weathers;
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
