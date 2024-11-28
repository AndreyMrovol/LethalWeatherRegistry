using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(AudioReverbTrigger))]
  public class AudioReverbTriggerPatches
  {
    [HarmonyTranspiler]
    [HarmonyPatch("ChangeAudioReverbForPlayer")]
    internal static IEnumerable<CodeInstruction> ChangeAudioReverbForPlayerPatch(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher matcher = new(instructions);

      Plugin.logger.LogDebugInstructionsFrom(matcher);

      // IL_191: call static TimeOfDay TimeOfDay::get_Instance()
      // IL_192: ldfld WeatherEffect[] TimeOfDay::effects
      // IL_193: call static TimeOfDay TimeOfDay::get_Instance()
      // IL_194: ldfld LevelWeatherType TimeOfDay::currentLevelWeather
      // IL_195: ldelem.ref NULL
      // IL_196: ldc.i4.1 NULL
      // IL_197: stfld bool WeatherEffect::effectEnabled

      var matcher2 = matcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "effects")),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentLevelWeather")),
        new CodeMatch(OpCodes.Ldelem_Ref),
        new CodeMatch(OpCodes.Ldc_I4_1),
        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(WeatherEffect), "effectEnabled"))
      );

      Plugin.logger.LogDebugInstructionsFrom(matcher2);
      Plugin.logger.LogWarning(matcher2.IsValid);

      matcher2.RemoveInstructions(7);

      return matcher.InstructionEnumeration();
    }
  }
}
