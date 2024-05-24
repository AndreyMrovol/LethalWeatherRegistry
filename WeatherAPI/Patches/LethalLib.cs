using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
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
      List<Weather> weathers = new List<Weather>();

      // list through all entries
      foreach (KeyValuePair<int, CustomWeather> LethalLibWeatherEntry in llWeathers)
      {
        CustomWeather llWeather = LethalLibWeatherEntry.Value;

        WeatherApiEffect effect =
          new(llWeather.weatherEffect.effectObject, llWeather.weatherEffect.effectPermanentObject)
          {
            SunAnimatorBool = llWeather.weatherEffect.sunAnimatorBool,
            DefaultVariable1 = llWeather.weatherVariable1,
            DefaultVariable2 = llWeather.weatherVariable2,
          };

        Weather weather = new(llWeather.name, effect) { VanillaWeatherType = (LevelWeatherType)LethalLibWeatherEntry.Key };
        weathers.Add(weather);

        WeatherManager.ModdedWeatherEnumExtension.Add(LethalLibWeatherEntry.Key, weather);
        // Get key
      }

      return weathers;
    }

    public static void Init()
    {
      Plugin.logger.LogWarning("Disabling LethalLib injections");

      // Get the patching method info
      var patchMethod = typeof(LethalLib.Modules.Weathers).GetMethod("ToStringHook");

      // Create a new detour for the method
      new Hook(patchMethod, TerminalStartPatch.ToStringHook);
    }

    // [HarmonyPatch("Init")]
    // [HarmonyTranspiler]
    // public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    // {
    //   var codeMatcher = new CodeMatcher(instructions);

    //   Plugin.logger.LogWarning("Transpiling LethalLib Init method");

    //   codeMatcher
    //     // Match the beginning of the IL code sequence
    //     .MatchForward(
    //       false,
    //       new CodeMatch(OpCodes.Ldtoken, typeof(Enum)),
    //       new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle))),
    //       new CodeMatch(OpCodes.Ldstr, "ToString"),
    //       new CodeMatch(OpCodes.Ldc_I4_0)
    //     // new CodeMatch(OpCodes.Newarr, typeof(Type)),
    //     // new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetMethod), new Type[] { typeof(string), typeof(Type[]) })),
    //     // new CodeMatch(OpCodes.Ldtoken, typeof(LethalLib.Modules.Weathers)),
    //     // new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle))),
    //     // new CodeMatch(OpCodes.Ldstr, "ToStringHook"),
    //     // new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Type), nameof(Type.GetMethod), new Type[] { typeof(string) })),
    //     // new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(Hook), new Type[] { typeof(MethodBase), typeof(MethodInfo) })),
    //     // new CodeMatch(OpCodes.Pop)
    //     )
    //     // Remove the matched instructions
    //     .RemoveInstructions(12)
    //     // Insert new instructions if needed
    //     .InsertAndAdvance(
    //     // Add your new instructions here if you want to replace the matched instructions
    //     // Example:
    //     // new CodeInstruction(OpCodes.Ldarg_0),
    //     // new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(YourType), nameof(YourMethod))),
    //     // new CodeInstruction(OpCodes.Ret)
    //     );

    //   return codeMatcher.InstructionEnumeration();
    // }

    [HarmonyPatch("StartOfRound_Awake")]
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
