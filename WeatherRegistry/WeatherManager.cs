using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;

namespace WeatherRegistry
{
  public static class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<RegistryWeather> RegisteredWeathers { get; internal set; } = [];
    public static List<LevelWeather> LevelWeathers { get; internal set; } = [];

    public static List<WeatherEffectOverride> WeatherEffectOverrides { get; internal set; } = [];

    // i would love to have weathers as an array with indexes corresponding to the enum values
    // but none is -1 so i have to do this
    public static List<RegistryWeather> Weathers { get; internal set; } = [];
    public static RegistryWeather NoneWeather { get; internal set; }

    public static Dictionary<int, RegistryWeather> ModdedWeatherEnumExtension = [];

    public static CurrentWeathers currentWeathers = new();

    public static void RegisterWeather(RegistryWeather weather)
    {
      RegisteredWeathers.Add(weather);
    }

    public static RegistryWeather GetWeather(LevelWeatherType levelWeatherType)
    {
      return Weathers.Find(weather => weather.VanillaWeatherType == levelWeatherType);

      // return Weathers[(int)levelWeatherType];
    }

    internal static void Reset()
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
      WeatherEffectOverrides.Clear();
      currentWeathers.Clear();

      Settings.ScreenMapColors.Clear();

      ConfigHelper.StringToWeather = null;

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
        .Select(x => x.weatherType)
        .Distinct()
        .ToList();

      // add None as a possible weather in front of the list
      possibleWeathers.Insert(0, LevelWeatherType.None);

      Plugin.logger.LogDebug($"Possible weathers: {string.Join("; ", possibleWeathers.Select(x => x.ToString()))}");

      if (possibleWeathers == null || possibleWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Level's random weathers are null");
        return [];
      }

      return possibleWeathers;
    }

    public static MrovLib.WeightHandler<RegistryWeather> GetPlanetWeightedList(SelectableLevel level)
    {
      MrovLib.WeightHandler<RegistryWeather> weightedList = new();
      MrovLib.Logger logger = WeatherCalculation.Logger;

      List<LevelWeatherType> weatherTypes = GetPlanetPossibleWeathers(level);

      if (weatherTypes == null || weatherTypes.Count() == 0)
      {
        Plugin.logger.LogError("Level's random weathers are null");
        return weightedList;
      }

      foreach (var weather in weatherTypes)
      {
        // clone the object
        RegistryWeather typeOfWeather = GetWeather(weather);

        var weatherWeight = typeOfWeather.GetWeight(level);

        weightedList.Add(typeOfWeather, weatherWeight);
      }

      return weightedList;
    }

    public static RegistryWeather GetCurrentWeather(SelectableLevel level)
    {
      if (currentWeathers.Contains(level))
      {
        return currentWeathers.GetLevelWeather(level);
      }
      else
      {
        return GetWeather(level.currentWeather);
      }
    }

    public static string GetCurrentWeatherName(SelectableLevel level)
    {
      return GetCurrentWeather(level).Name;
    }

    internal static AnimationClip GetWeatherAnimationClip(LevelWeatherType weatherType)
    {
      return GetWeather(weatherType).AnimationClip;
    }

    public static WeatherEffectOverride GetCurrentWeatherOverride(SelectableLevel level, RegistryWeather weather)
    {
      weather.WeatherEffectOverrides.TryGetValue(level, out WeatherEffectOverride weatherEffectOverride);

      return weatherEffectOverride;
    }
  }
}
