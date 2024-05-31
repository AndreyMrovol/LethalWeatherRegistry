using System;
using System.Collections.Generic;
using System.Linq;

namespace WeatherAPI
{
  public class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers = [];
    public static List<LevelWeather> LevelWeathers = [];

    // i would love to have weathers as an array with indexes corresponding to the enum values
    // but none is -1 so i have to do this
    public static List<Weather> Weathers = [];
    public static Weather NoneWeather;

    public static Dictionary<int, Weather> ModdedWeatherEnumExtension = [];

    public static Dictionary<SelectableLevel, Weather> CurrentWeathers = [];

    public static Weather GetWeather(LevelWeatherType levelWeatherType)
    {
      return Weathers.Find(weather => weather.VanillaWeatherType == levelWeatherType);

      // return Weathers[(int)levelWeatherType];
    }

    public static void Reset()
    {
      IsSetupFinished = false;

      // RegisteredWeathers.Clear();
      LevelWeathers.Clear();
      Weathers.Clear();
      ModdedWeatherEnumExtension.Clear();

      // RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherAPI);
    }

    public static string LevelWeatherTypeEnumHook(Func<Enum, string> orig, Enum self)
    {
      if (self.GetType() == typeof(LevelWeatherType))
      {
        // Plugin.logger.LogDebug($"LevelWeatherTypeEnumHook");
        if (WeatherManager.ModdedWeatherEnumExtension.ContainsKey((int)(LevelWeatherType)self))
        {
          return WeatherManager.ModdedWeatherEnumExtension[(int)(LevelWeatherType)self].name;
        }
      }

      return orig(self);
    }

    // weathertweaks copy-paste:
    internal static List<LevelWeatherType> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      Plugin.logger.LogDebug($"Getting possible weathers for {level.PlanetName}");

      List<LevelWeatherType> possibleWeathers = level
        .randomWeathers.ToList()
        .Where(randomWeather => randomWeather.weatherType != LevelWeatherType.None)
        .ToList()
        .Select(x => x.weatherType)
        .ToList();

      // add None as a possible weather in front of the list
      possibleWeathers.Insert(0, LevelWeatherType.None);

      Plugin.logger.LogInfo($"Possible weathers: {string.Join("; ", possibleWeathers.Select(x => x.ToString()))}");

      if (possibleWeathers == null || possibleWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Level's random weathers are null");
        return [];
      }

      return possibleWeathers;
    }

    internal static List<LevelWeatherType> GetPlanetWeightedList(SelectableLevel level, Dictionary<LevelWeatherType, int> weights)
    {
      var weatherList = new List<LevelWeatherType>();

      List<LevelWeatherType> weatherTypes = GetPlanetPossibleWeathers(level);

      if (weatherTypes.Count == 0)
      {
        return [];
      }

      foreach (var weather in weatherTypes)
      {
        // clone the object
        Weather typeOfWeather = GetWeather(weather);

        var weatherWeight = weights.TryGetValue(typeOfWeather.VanillaWeatherType, out int weight) ? weight : typeOfWeather.DefaultWeight;

        Plugin.logger.LogDebug($"{typeOfWeather.Name} has weight {weatherWeight}");

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weather);
        }
      }

      return weatherList;
    }

    internal static Weather GetCurrentWeather(SelectableLevel level)
    {
      if (CurrentWeathers.TryGetValue(level, out Weather currentWeather))
      {
        return currentWeather;
      }

      return NoneWeather;
    }
  }
}
