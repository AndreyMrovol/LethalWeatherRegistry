using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Netcode;

namespace WeatherAPI
{
  internal class WeatherSync
  {
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
  }
}
