using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;
using WeatherRegistry.Managers;
using WeatherRegistry.Modules;

namespace WeatherRegistry
{
  public static class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers { get; internal set; } = [];

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
          // GameObject.Destroy(weather);
        }
      });

      Weathers.Clear();
      WeathersDictionary.Clear();
      ModdedWeatherEnumExtension.Clear();
      CurrentWeathers.Clear();

      OverridesManager.WeatherEffectOverrides.Clear();

      Settings.ScreenMapColors.Clear();

      ConfigHelper.StringToWeather = null;

      RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherRegistry && weather.Origin != WeatherOrigin.WeatherTweaks);

      Networking.WeatherLevelData.LatestWeathersReceived = [];
    }

    public static string WeatherDisplayOverride(SelectableLevel level)
    {
      return string.Empty;
    }

    #region WeightsManager

    internal static List<LevelWeatherType> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      return WeightsManager.GetPlanetPossibleWeathers(level);
    }

    public static Utils.WeightHandler<Weather, WeatherWeightType> GetPlanetWeightedList(SelectableLevel level)
    {
      return WeightsManager.GetPlanetWeightedList(level);
    }

    public static (int weight, WeatherWeightType type) GetWeatherWeightForLevel(SelectableLevel level, Weather weather)
    {
      return WeightsManager.GetWeatherWeightForLevel(level, weather);
    }

    #endregion

    #region OverridesManager

    [Obsolete("Use OverridesManager.GetCurrentWeatherOverride instead")]
    public static WeatherEffectOverride GetCurrentWeatherOverride(SelectableLevel level, Weather weather)
    {
      return OverridesManager.GetCurrentWeatherOverride(level, weather);
    }

    [Obsolete("Use OverridesManager.WeatherEffectOverrides instead")]
    public static List<WeatherEffectOverride> WeatherEffectOverrides => OverridesManager.WeatherEffectOverrides;

    #endregion

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
