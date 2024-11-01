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
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;
using static LethalLib.Modules.Weathers;

namespace WeatherRegistry.Patches
{
  public class LethalLibPatch
  {
    public static Dictionary<int, CustomWeather> GetLethalLibWeathers()
    {
      // Get all the weathers from LethalLib
      return LethalLib.Modules.Weathers.customWeathers;
    }

    public static List<RegistryWeather> ConvertLLWeathers()
    {
      Dictionary<int, CustomWeather> llWeathers = GetLethalLibWeathers();
      List<RegistryWeather> weathers = [];

      // list through all entries
      foreach (KeyValuePair<int, CustomWeather> LethalLibWeatherEntry in llWeathers)
      {
        CustomWeather llWeather = LethalLibWeatherEntry.Value;

        WeatherEffectDefinition effect =
          new(llWeather.weatherEffect) { DefaultVariable1 = llWeather.weatherVariable1, DefaultVariable2 = llWeather.weatherVariable2, };

        WeatherDefinition weatherDefinition =
          new()
          {
            Name = llWeather.name,
            Effect = effect,
            Color = Defaults.LethalLibColor,
            Configuration = { DefaultWeight = new(50), }
          };

        RegistryWeather weather =
          new(weatherDefinition)
          {
            VanillaWeatherType = (LevelWeatherType)LethalLibWeatherEntry.Key,
            Origin = WeatherOrigin.LethalLib,
            // Color = Defaults.LethalLibColor,
          };

        // weathers.Add(weather);

        WeatherManager.ModdedWeatherEnumExtension.Add(LethalLibWeatherEntry.Key, weather);
        // Get key
      }

      return weathers;
    }

    public static void Init()
    {
      Plugin.logger.LogDebug("Disabling LethalLib injections");

      // Access the field: private static Hook? weatherEnumHook;
      // added in lethallib 0.16.0
      var weatherEnumHookField = typeof(LethalLib.Modules.Weathers).GetField("weatherEnumHook", BindingFlags.NonPublic | BindingFlags.Static);
      Hook weatherEnumHook = (Hook)weatherEnumHookField.GetValue(null);
      weatherEnumHook.Undo();

      Plugin.harmony.Patch(
        AccessTools.Method(typeof(LethalLib.Modules.Weathers), "RegisterLevelWeathers_StartOfRound_Awake"),
        prefix: new HarmonyMethod(typeof(LethalLibPatch), nameof(StartOfRoundAwakePrefix))
      );

      Plugin.harmony.Patch(
        AccessTools.Method(typeof(LethalLib.Modules.Weathers), "TimeOfDay_Awake"),
        prefix: new HarmonyMethod(typeof(LethalLibPatch), nameof(TimeOfDayAwakePrefix))
      );
    }

    internal static bool StartOfRoundAwakePrefix(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
      Plugin.debugLogger.LogDebug("Skipping LethalLib StartOfRound method");
      orig(self);
      return false;
    }

    internal static bool TimeOfDayAwakePrefix(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
    {
      Plugin.debugLogger.LogDebug("Skipping LethalLib TimeOfDay method");
      orig(self);
      return false;
    }
  }
}
