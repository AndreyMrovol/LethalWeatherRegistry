using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(AudioReverbTrigger))]
  public class AudioReverbTriggerPatches
  {
    private static readonly MrovLib.Logger logger = new("WR AudioReverbTrigger");

    [HarmonyTranspiler]
    [HarmonyPatch("ChangeAudioReverbForPlayer")]
    internal static IEnumerable<CodeInstruction> ChangeAudioReverbForPlayerPatch(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher matcher = new(instructions);

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

      matcher2.RemoveInstructions(7);

      // call our method in place of removed code for extended logging
      matcher2.Insert(
        new CodeInstruction(OpCodes.Ldarg_0),
        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AudioReverbTriggerPatches), nameof(ChangeReverbRegistryCheck)))
      );

      return matcher.InstructionEnumeration();
    }

    public static void ChangeReverbRegistryCheck(AudioReverbTrigger audioReverbTrigger)
    {
      logger.LogWarning($"---CheckIfShouldEnable Ran!---");

      if (audioReverbTrigger == null)
      {
        return;
      }

      if (audioReverbTrigger.enableCurrentLevelWeather && TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None)
      {
        logger.LogDebug($"AudioReverbTrigger: {audioReverbTrigger}");
        logger.LogDebug($"EnableCurrentLevelWeather: {audioReverbTrigger.enableCurrentLevelWeather}");
        logger.LogDebug($"CurrentLevelWeather: {TimeOfDay.Instance.currentLevelWeather}");

        logger.LogWarning($"Currently enabled effects: {string.Join(", ", WeatherManager.CurrentEffectTypes)}");

        // Enable the current level weather effects
        WeatherEffectController.EnableCurrentWeatherEffects();
      }
    }
  }
}
