using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LethalLib;
using LethalLib.Extras;
using MonoMod.RuntimeDetour;
using UnityEngine;
using static LethalLib.Modules.Weathers;

namespace WeatherAPI.Patches
{
  [HarmonyPatch(typeof(LethalLib.Modules.Weathers))]
  public class LethalLibPatch
  {
    public static Dictionary<int, CustomWeather> GetLethalLibWeathers()
    {
      // Get all the weathers from LethalLib
      return LethalLib.Modules.Weathers.customWeathers;
    }

    public static List<Weather> ConvertLLWeathers()
    {
      Dictionary<int, CustomWeather> llWeathers = GetLethalLibWeathers();
      List<Weather> weathers = [];

      // list through all entries
      foreach (KeyValuePair<int, CustomWeather> LethalLibWeatherEntry in llWeathers)
      {
        CustomWeather llWeather = LethalLibWeatherEntry.Value;

        ImprovedWeatherEffect effect =
          new(llWeather.weatherEffect.effectObject, llWeather.weatherEffect.effectPermanentObject)
          {
            SunAnimatorBool = llWeather.weatherEffect.sunAnimatorBool,
            DefaultVariable1 = llWeather.weatherVariable1,
            DefaultVariable2 = llWeather.weatherVariable2,
          };

        Weather weather =
          new(llWeather.name, effect)
          {
            VanillaWeatherType = (LevelWeatherType)LethalLibWeatherEntry.Key,
            Origin = WeatherOrigin.LethalLib,
            Color = Defaults.LethalLibColor,
            DefaultWeight = 50,
          };
        weathers.Add(weather);

        WeatherManager.ModdedWeatherEnumExtension.Add(LethalLibWeatherEntry.Key, weather);
        // Get key
      }

      return weathers;
    }

    public static void Init()
    {
      Plugin.logger.LogWarning("Disabling LethalLib injections");

      // Access the field: private static Hook? weatherEnumHook;
      // added in lethallib 0.16.0
      var weatherEnumHookField = typeof(LethalLib.Modules.Weathers).GetField("weatherEnumHook", BindingFlags.NonPublic | BindingFlags.Static);
      Hook weatherEnumHook = (Hook)weatherEnumHookField.GetValue(null);
      weatherEnumHook.Undo();
    }

    [HarmonyPatch("RegisterLevelWeathers_StartOfRound_Awake")]
    [HarmonyPrefix]
    internal static bool StartOfRoundAwakePrefix(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
      Plugin.logger.LogWarning("Skipping LethalLib StartOfRound method");
      orig(self);
      return false;
    }

    [HarmonyPatch("TimeOfDay_Awake")]
    [HarmonyPrefix]
    internal static bool TimeOfDayAwakePrefix(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
    {
      Plugin.logger.LogWarning("Skipping LethalLib TimeOfDay method");
      orig(self);
      return false;
    }
  }
}
