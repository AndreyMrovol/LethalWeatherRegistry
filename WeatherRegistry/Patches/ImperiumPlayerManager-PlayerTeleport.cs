using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Imperium.API.Types.Networking;

namespace WeatherRegistry.Patches
{
  public static partial class ImperiumPatches
  {
    public static class ImperiumPlayerManagerPatch
    {
      public static void TeleportPlayerPrefixPatch(TeleportPlayerRequest request)
      {
        Plugin.debugLogger.LogDebug($"TeleportPlayer called with playerId: {request.PlayerId} and destination: {request.Destination}");
        Plugin.debugLogger.LogDebug($"Is player script inside? {StartOfRound.Instance.allPlayerScripts[request.PlayerId].isInsideFactory}");
      }

      public static IEnumerable<CodeInstruction> TeleportPlayerTranspilerPatch(IEnumerable<CodeInstruction> instructions)
      {
        CodeMatcher matcher = new(instructions);

        // 82	00E4	call	class ['Assembly-CSharp']TimeOfDay ['Assembly-CSharp']TimeOfDay::get_Instance()
        // 83	00E9	ldc.i4.0
        // 84	00EA	callvirt	instance void ['Assembly-CSharp']TimeOfDay::DisableAllWeather(bool)

        matcher.MatchForward(
          false,
          new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
          new CodeMatch(OpCodes.Ldc_I4_0),
          new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TimeOfDay), "DisableAllWeather"))
        );

        matcher.RemoveInstructions(3);

        return matcher.InstructionEnumeration();
      }

      public static void TeleportPlayerPostfixPatch(TeleportPlayerRequest request)
      {
        Plugin.debugLogger.LogDebug(
          $"Postfix: Is player script inside? {StartOfRound.Instance.allPlayerScripts[request.PlayerId].isInsideFactory}"
        );

        Settings.IsPlayerInside = StartOfRound.Instance.allPlayerScripts[request.PlayerId].isInsideFactory;

        if (Settings.IsPlayerInside)
        {
          Plugin.debugLogger.LogDebug("Player is inside, disabling all weather effects.");
          TimeOfDay.Instance.DisableAllWeather(false);
        }
        else
        {
          Plugin.debugLogger.LogDebug("Player is outside, enabling all weather effects.");
          WeatherEffectController.EnableCurrentWeatherEffects();
        }
      }
    }
  }
}
