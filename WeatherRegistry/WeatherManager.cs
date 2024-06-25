using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeatherRegistry
{
  public class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers { get; internal set; } = [];
    public static List<LevelWeather> LevelWeathers { get; internal set; } = [];

    // i would love to have weathers as an array with indexes corresponding to the enum values
    // but none is -1 so i have to do this
    public static List<Weather> Weathers { get; internal set; } = [];
    public static Weather NoneWeather { get; internal set; }

    public static Dictionary<int, Weather> ModdedWeatherEnumExtension = [];

    public static Dictionary<SelectableLevel, Weather> CurrentWeathers = [];

    public static void RegisterWeather(Weather weather)
    {
      RegisteredWeathers.Add(weather);
    }

    public static Weather GetWeather(LevelWeatherType levelWeatherType)
    {
      return Weathers.Find(weather => weather.VanillaWeatherType == levelWeatherType);

      // return Weathers[(int)levelWeatherType];
    }

    public static void Reset()
    {
      IsSetupFinished = false;

      Weathers.ForEach(weather =>
      {
        if (weather.Origin != WeatherOrigin.WeatherRegistry)
        {
          GameObject.Destroy(weather.Effect);
          GameObject.Destroy(weather);
        }
      });

      // RegisteredWeathers.Clear();
      LevelWeathers.Clear();
      Weathers.Clear();
      ModdedWeatherEnumExtension.Clear();
      CurrentWeathers.Clear();

      Settings.ScreenMapColors.Clear();

      RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherRegistry);
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

    internal static List<LevelWeatherType> GetPlanetWeightedList(SelectableLevel level)
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

        // we have 3 weights possible:
        // 1. level weight
        // 2. weather-weather weights
        // 3. default weight
        // we want to execute them in this exact order

        Dictionary<SelectableLevel, int> levelWeights = typeOfWeather.LevelWeights;
        Dictionary<LevelWeatherType, int> weatherWeights = typeOfWeather.WeatherWeights;

        var weatherWeight = typeOfWeather.DefaultWeight;

        if (levelWeights.TryGetValue(level, out int levelWeight))
        {
          // (1) => level weight
          Plugin.logger.LogDebug($"{typeOfWeather.Name} has level weight {levelWeight}");
          weatherWeight = levelWeight;
        }
        // try to get previous day weather (so - at this point - the current one)
        // but not on first day because that's completely random
        else if (
          weatherWeights.TryGetValue(level.currentWeather, out int weatherWeightFromWeather)
          && StartOfRound.Instance.gameStats.daysSpent != 0
        )
        {
          // (2) => weather-weather weights
          Plugin.logger.LogDebug($"{typeOfWeather.Name} has weather>weather weight {weatherWeightFromWeather}");
          weatherWeight = weatherWeightFromWeather;
        }
        else
        {
          Plugin.logger.LogDebug($"{typeOfWeather.Name} has default weight {weatherWeight}");
        }

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weather);
        }
      }

      return weatherList;
    }

    internal static Weather GetCurrentWeather(SelectableLevel level)
    {
      if (CurrentWeathers.ContainsKey(level))
      {
        return CurrentWeathers[level];
      }
      else
      {
        return GetWeather(level.currentWeather);
      }
    }

    internal static string GetCurrentWeatherName(SelectableLevel level)
    {
      return GetCurrentWeather(level).Name;
    }

    internal static AnimationClip GetWeatherAnimationClip(LevelWeatherType weatherType)
    {
      return GetWeather(weatherType).AnimationClip;
    }
  }
}
