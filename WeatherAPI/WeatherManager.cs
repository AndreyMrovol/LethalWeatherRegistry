using System;
using System.Collections.Generic;

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

    public static void Reset()
    {
      // RegisteredWeathers.Clear();
      LevelWeathers.Clear();
      Weathers.Clear();
      ModdedWeatherEnumExtension.Clear();

      RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherAPI);
    }

    public static string LevelWeatherTypeEnumHook(Func<Enum, string> orig, Enum self)
    {
      if (self.GetType() == typeof(LevelWeatherType))
      {
        Plugin.logger.LogDebug($"LevelWeatherTypeEnumHook");
        if (WeatherManager.ModdedWeatherEnumExtension.ContainsKey((int)(LevelWeatherType)self))
        {
          return WeatherManager.ModdedWeatherEnumExtension[(int)(LevelWeatherType)self].name;
        }
      }

      return orig(self);
    }
  }
}
