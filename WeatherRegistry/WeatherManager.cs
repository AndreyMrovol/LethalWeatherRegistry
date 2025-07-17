using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public static class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers { get; internal set; } = [];

    [Obsolete]
    public static List<LevelWeather> LevelWeathers { get; internal set; } = [];

    [Obsolete("Use WeatherOverrideManager.WeatherEffectOverrides instead")]
    public static List<WeatherEffectOverride> WeatherEffectOverrides => WeatherOverrideManager.WeatherEffectOverrides;

    public static List<WeatherMatcher> WeatherMatchers { get; internal set; } = [];

    // i would love to have weathers as an array with indexes corresponding to the enum values
    // but none is -1 so i have to do this
    public static List<Weather> Weathers => WeathersDictionary.Values.ToList();
    public static Dictionary<LevelWeatherType, Weather> WeathersDictionary { get; internal set; } = [];

    public static Weather NoneWeather { get; internal set; }

    public static List<LevelWeatherType> LevelWeatherTypes => Weathers.Select(weather => weather.VanillaWeatherType).ToList();

    public static Dictionary<int, Weather> ModdedWeatherEnumExtension = [];

    public static CurrentWeathers CurrentWeathers = new();

    public static List<LevelWeatherType> CurrentEffectTypes
    {
      get
      {
        return Weathers
          .Where(weather => weather.Effect != null)
          .Where(weather => weather.Effect.EffectEnabled)
          .Select(weather => weather.VanillaWeatherType)
          .ToList();
      }
    }

    public static void RegisterWeather(Weather weather)
    {
      RegisteredWeathers.Add(weather);
    }

    public static Weather GetWeather(LevelWeatherType levelWeatherType)
    {
      if (WeathersDictionary.ContainsKey(levelWeatherType))
      {
        return WeathersDictionary[levelWeatherType];
      }
      else
      {
        // if this is called at any point, we're fucking COOKED
        Plugin.logger.LogWarning($"Weather {levelWeatherType} not found in dictionary");
        return null;
      }
    }

    public static List<Weather> GetWeathers()
    {
      return Weathers;
    }

    internal static void Reset()
    {
      IsSetupFinished = false;

      Weathers.ForEach(weather =>
      {
        if (weather.Origin != WeatherOrigin.WeatherRegistry && weather.Origin != WeatherOrigin.WeatherTweaks)
        {
          GameObject.Destroy(weather.Effect);
          GameObject.Destroy(weather);
        }
      });

      // RegisteredWeathers.Clear();
      LevelWeathers.Clear();
      Weathers.Clear();
      WeathersDictionary.Clear();
      ModdedWeatherEnumExtension.Clear();
      CurrentWeathers.Clear();

      WeatherOverrideManager.WeatherEffectOverrides.Clear();

      Settings.ScreenMapColors.Clear();

      ConfigHelper.StringToWeather = null;

      RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherRegistry && weather.Origin != WeatherOrigin.WeatherTweaks);

      Networking.WeatherLevelData.LatestWeathersReceived = [];
    }

    public static string WeatherDisplayOverride(SelectableLevel level)
    {
      return string.Empty;
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

    public static MrovLib.WeightHandler<Weather> GetPlanetWeightedList(SelectableLevel level)
    {
      MrovLib.WeightHandler<Weather> weightedList = new();
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
        Weather typeOfWeather = GetWeather(weather);

        var weatherWeight = typeOfWeather.GetWeight(level);

        weightedList.Add(typeOfWeather, weatherWeight);
      }

      return weightedList;
    }

    public static Weather GetCurrentWeather(SelectableLevel level)
    {
      if (!Settings.SetupFinished)
      {
        Plugin.logger.LogWarning("Something is trying to get the current weather before setup is finished!");
        return null;
      }

      return CurrentWeathers.GetLevelWeather(level);
    }

    public static Weather GetCurrentLevelWeather()
    {
      return GetCurrentWeather(StartOfRound.Instance.currentLevel);
    }

    public static string GetCurrentWeatherName(SelectableLevel level, bool ignoreOverride = false)
    {
      string weatherNameOverride = WeatherDisplayOverride(level);
      if (weatherNameOverride != string.Empty && !ignoreOverride)
      {
        return weatherNameOverride;
      }

      return GetCurrentWeather(level).Name;
    }

    [Obsolete]
    internal static AnimationClip GetWeatherAnimationClip(LevelWeatherType weatherType)
    {
      return GetWeather(weatherType).AnimationClip;
    }

    [Obsolete("Use WeatherOverrideManager.GetCurrentWeatherOverride instead")]
    public static WeatherEffectOverride GetCurrentWeatherOverride(SelectableLevel level, Weather weather)
    {
      return WeatherOverrideManager.GetCurrentWeatherOverride(level, weather);
    }

    public static string GetWeatherList()
    {
      List<WeatherListData> weathers = [];

      foreach (var weather in Weathers)
      {
        weathers.Add(new WeatherListData { WeatherID = ((int)weather.VanillaWeatherType).ToString(), WeatherName = weather.Name });
      }

      return JsonConvert.SerializeObject(
        weathers,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );
    }
  }
}
