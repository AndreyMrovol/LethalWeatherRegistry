using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Newtonsoft.Json;
using WeatherRegistry.Definitions;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  [HarmonyPatch(typeof(StartOfRound))]
  internal class OpeningDoorsSequencePatch
  {
    // https://github.com/SylviBlossom/LC-SimpleWeatherDisplay/blob/2d252b92dcd4d8ef259b8072d9339ff5ccdc4d0b/src/Plugin.cs#L127-L153
    // it started from that ILCursor mess
    // and now it's worse

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
    internal static IEnumerable<CodeInstruction> StartOfRound_openingDoorsSequence(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher matcher = new(instructions);

      // IL_105: call static TimeOfDay TimeOfDay::get_Instance()
      // IL_106: ldfld WeatherEffect[] TimeOfDay::effects
      // IL_107: ldloc.1 NULL
      // IL_108: ldfld SelectableLevel StartOfRound::currentLevel
      // IL_109: ldfld LevelWeatherType SelectableLevel::currentWeather
      // IL_110: ldelem.ref NULL
      // IL_111: stloc.2 NULL
      // IL_112: ldloc.2 NULL
      // IL_113: ldc.i4.1 NULL
      // IL_114: stfld bool WeatherEffect::effectEnabled
      // IL_115: ldloc.2 NULL
      // IL_116: ldfld UnityEngine.GameObject WeatherEffect::effectPermanentObject
      // IL_117: ldnull NULL
      // IL_118: call static bool UnityEngine.Object::op_Inequality(UnityEngine.Object x, UnityEngine.Object y)
      // IL_119: brfalse Label13
      // IL_120: ldloc.2 NULL
      // IL_121: ldfld UnityEngine.GameObject WeatherEffect::effectPermanentObject
      // IL_122: ldc.i4.1 NULL
      // IL_123: callvirt void UnityEngine.GameObject::SetActive(bool value)

      var matcher2 = matcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "effects")),
        new CodeMatch(OpCodes.Ldloc_1),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), "currentLevel")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SelectableLevel), "currentWeather")),
        new CodeMatch(OpCodes.Ldelem_Ref),
        new CodeMatch(OpCodes.Stloc_2),
        new CodeMatch(OpCodes.Ldloc_2),
        new CodeMatch(OpCodes.Ldc_I4_1),
        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(WeatherEffect), "effectEnabled")),
        new CodeMatch(OpCodes.Ldloc_2),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WeatherEffect), "effectPermanentObject")),
        new CodeMatch(OpCodes.Ldnull),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality")),
        new CodeMatch(OpCodes.Brfalse),
        new CodeMatch(OpCodes.Ldloc_2),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WeatherEffect), "effectPermanentObject")),
        new CodeMatch(OpCodes.Ldc_I4_1),
        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(UnityEngine.GameObject), "SetActive"))
      );

      matcher2.RemoveInstructions(19);

      // IL_155: call static TimeOfDay TimeOfDay::get_Instance()
      // IL_156: ldfld WeatherEffect[] TimeOfDay::effects
      // IL_157: call static TimeOfDay TimeOfDay::get_Instance()
      // IL_158: ldfld LevelWeatherType TimeOfDay::currentLevelWeather
      // IL_159: ldelem.ref NULL
      // IL_160: ldc.i4.1 NULL
      // IL_161: stfld bool WeatherEffect::effectEnabled

      var matcher3 = matcher2.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "effects")),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentLevelWeather")),
        new CodeMatch(OpCodes.Ldelem_Ref),
        new CodeMatch(OpCodes.Ldc_I4_1),
        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(WeatherEffect), "effectEnabled"))
      );

      matcher3.RemoveInstructions(7);

      // Insert weather effect delegates
      matcher3
        .Advance(1)
        .Insert(
          new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OpeningDoorsSequencePatch), nameof(RunWeatherPatches))),
          new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OpeningDoorsSequencePatch), nameof(SetWeatherEffects)))
        );

      matcher3
        .MatchForward(
          false,
          // Match: currentLevel.LevelDescription
          new CodeMatch(OpCodes.Ldloc_1),
          new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), "currentLevel")),
          new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SelectableLevel), "LevelDescription"))
        )
        // Don't remove instructions - we need the description string
        .Advance(3)
        .Insert(
          // Load StartOfRound instance as second parameter
          new CodeInstruction(OpCodes.Ldloc_1),
          // Call our method with (string, StartOfRound) parameters
          new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OpeningDoorsSequencePatch), nameof(ModifyDescription)))
        );

      return matcher.InstructionEnumeration();
    }

    private static string ModifyDescription(string description, StartOfRound instance)
    {
      var weatherName =
        instance.currentLevel.currentWeather != LevelWeatherType.None ? WeatherManager.GetCurrentWeatherName(instance.currentLevel) : "Clear";
      var weatherLine = $"WEATHER: {weatherName}";

      return $"{weatherLine}\n{description}";
    }

    internal static void RunWeatherPatches()
    {
      TimeOfDay.Instance.nextTimeSync = 0;
    }

    internal static void SetWeatherEffects()
    {
      SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
      Weather currentWeather = WeatherManager.GetCurrentWeather(currentLevel);

      if (StartOfRound.Instance.IsHost)
      {
        WeatherSync.Instance.SetWeatherEffectOnHost(currentWeather.VanillaWeatherType);
      }

      // WeatherEffectController.SetWeatherEffects(currentWeather);

      Plugin.logger.LogDebug(
        $"Landing at {ConfigHelper.GetNumberlessName(currentLevel)} with weather {currentWeather.Name} ({currentWeather.VanillaWeatherType})"
      );

      EventManager.ShipLanding.Invoke((currentLevel, currentWeather));
    }
  }
}
