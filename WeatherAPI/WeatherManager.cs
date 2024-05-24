using System.Collections.Generic;

// using WeatherAPI.Definitions;

namespace WeatherAPI
{
  public class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers = [];
    public static List<LevelWeather> LevelWeathers = [];

    public static List<Weather> Weathers = [];

    public static Dictionary<int, Weather> ModdedWeatherEnumExtension = [];

    public static Weather GetWeather(LevelWeatherType levelWeatherType)
    {
      return Weathers.Find(weather => weather.VanillaWeatherType == levelWeatherType);
    }
  }
}
