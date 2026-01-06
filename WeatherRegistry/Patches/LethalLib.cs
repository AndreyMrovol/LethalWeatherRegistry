using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using WeatherRegistry.Enums;
using WeatherRegistry.Utils;
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
            name = llWeather.name,
            SunAnimatorBool = llWeather.weatherEffect.sunAnimatorBool,
            DefaultVariable1 = llWeather.weatherVariable1,
            DefaultVariable2 = llWeather.weatherVariable2,
          };

        Weather weather =
          new(llWeather.name, effect)
          {
            VanillaWeatherType = (LevelWeatherType)LethalLibWeatherEntry.Key,
            Origin = WeatherOrigin.LethalLib,
            ColorGradient = ColorConverter.ToTMPColorGradient(Defaults.LethalLibColor),
            Config =
            {
              DefaultWeight = new(50),
              FilteringOption = new(FilteringOption.Include),
              LevelFilters = new(string.Join(";", llWeather.levels)),
            }
          };

        weathers.Add(weather);

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
      Plugin.logger.LogDebug("Skipping LethalLib StartOfRound method");
      orig(self);
      return false;
    }

    internal static bool TimeOfDayAwakePrefix(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
    {
      Plugin.logger.LogDebug("Skipping LethalLib TimeOfDay method");
      orig(self);
      return false;
    }
  }
}
