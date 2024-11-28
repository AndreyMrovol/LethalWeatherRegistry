using System.Linq;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Networking
{
  public class WeatherLevelData
  {
    public static WeatherSyncData[] LatestWeathersReceived;

    internal static void WeathersReceived(WeatherSyncDataWrapper previousValue, WeatherSyncDataWrapper newValue)
    {
      Plugin.logger.LogDebug($"Weathers received: [{string.Join("; ", newValue.Weathers.Select(w => w.ToString()))}]");

      ApplyReceivedWeathers(newValue.Weathers);
    }

    internal static void ApplyReceivedWeathers(WeatherSyncData[] weathers)
    {
      Plugin.logger.LogDebug($"Weathers to apply: {weathers.Length} entries");

      if (ShouldSkipWeatherUpdate(weathers))
      {
        return;
      }

      UpdateLevelWeathers(weathers);
      LatestWeathersReceived = weathers;

      WeatherManager.currentWeathers.Refresh();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    internal static bool ShouldSkipWeatherUpdate(WeatherSyncData[] weathers)
    {
      if (LatestWeathersReceived != null && LatestWeathersReceived.SequenceEqual(weathers))
      {
        Plugin.logger.LogDebug("Weathers are the same as last ones, skipping");
        return true;
      }

      if (weathers.Length == 0)
      {
        Plugin.logger.LogDebug("Weathers are not set, skipping");
        return true;
      }

      return false;
    }

    internal static void UpdateLevelWeathers(WeatherSyncData[] weathers)
    {
      foreach (SelectableLevel level in StartOfRound.Instance.levels)
      {
        var weatherData = weathers.FirstOrDefault(w => w.LevelName.ToString() == level.PlanetName);
        if (weatherData.LevelName.ToString() != "")
        {
          level.currentWeather = weatherData.Weather;
        }
      }
    }
  }
}
