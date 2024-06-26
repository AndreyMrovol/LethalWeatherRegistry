using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace WeatherRegistry
{
  public static class WeatherManager
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

      ConfigHelper.StringToWeather = null;
      ConfigHelper.StringToLevel = null;

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
      List<LevelWeatherType> possibleWeathers = level
        .randomWeathers.Where(randomWeather => randomWeather.weatherType != LevelWeatherType.None)
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

    internal static Dictionary<Weather, int> GetPlanetWeightedList(SelectableLevel level)
    {
      Dictionary<Weather, int> weightedList = [];

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

        weightedList.Add(typeOfWeather, weatherWeight);
      }

      return weightedList;
    }

    internal static (Weather, Dictionary<Weather, int>) PickNewRandomWeather(SelectableLevel level)
    {
      Dictionary<Weather, int> weightedList = GetPlanetWeightedList(level);

      if (weightedList.Count == 0)
      {
        Plugin.logger.LogError("Weighted list is empty");
        return (WeatherManager.NoneWeather, weightedList);
      }

      // sum of all weights
      int sum = weightedList.Values.Sum();

      if (sum <= 0)
      {
        Plugin.logger.LogError("Sum of all weights is 0");
        return (WeatherManager.NoneWeather, weightedList);
      }

      int roll = new System.Random().Next(0, sum);
      int total = 0;

      foreach (KeyValuePair<Weather, int> pair in weightedList.OrderByDescending(v => v.Value))
      {
        total += pair.Value;

        if (roll <= total)
        {
          return (pair.Key, weightedList);
        }
      }

      return (weightedList.Keys.FirstOrDefault(), weightedList);
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
