using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(RedLocustBees))]
  public static class RedLocustBeesSpawnPatch
  {
    [HarmonyAfter("butterystancakes.lethalcompany.butteryfixes")]
    [HarmonyPatch("SpawnHiveNearEnemy")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SpawnHiveNearEnemyTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      var matcher = new CodeMatcher(instructions);

      matcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Ldloc_3), // num (hive value)
        new CodeMatch(OpCodes.Ldloc_1) // original instruction
      );

      if (matcher.IsInvalid)
      {
        Plugin.logger.LogError("Failed to find Ldloc_3 followed by Ldloc_1 in SpawnHiveNearEnemy!");
        return instructions;
      }

      // Move to Ldloc_1
      matcher.Advance(1);

      // Check if there's a Call instruction after this Ldloc_1 (ButteryFixes inserted code)
      var nextInstruction = matcher.Instruction;
      matcher.Advance(1);
      var instructionAfterNext = matcher.Instruction;

      if (nextInstruction.opcode == OpCodes.Ldloc_1 && instructionAfterNext.opcode == OpCodes.Call)
      {
        // ButteryFixes is present: pattern is Ldloc_3, Ldloc_1, Ldloc_1, Call, (original Ldloc_1)
        matcher.Advance(1); // Move past the Call
        Plugin.logger.LogInfo("Detected ButteryFixes patch, inserting after RerollHivePrice call");
      }
      else
      {
        // Vanilla: pattern is just Ldloc_3, Ldloc_1
        matcher.Advance(-1); // Go back to Ldloc_1
        Plugin.logger.LogInfo("No ButteryFixes patch detected, inserting at vanilla position");
      }

      matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RedLocustBeesSpawnPatch), nameof(GetAdjustedHiveValue))));

      Plugin.logger.LogInfo("Successfully patched SpawnHiveNearEnemy with weather multiplier adjustment!");
      return matcher.InstructionEnumeration();
    }

    private static int GetAdjustedHiveValue(int originalValue)
    {
      Weather currentWeather = WeatherManager.GetCurrentLevelWeather();

      int adjustedValue = Mathf.CeilToInt(originalValue * currentWeather.ScrapValueMultiplier);

      Plugin.logger.LogDebug(
        $"Adjusted hive scrap value: {originalValue} -> {adjustedValue} (multiplier: {currentWeather.ScrapValueMultiplier})"
      );

      return adjustedValue;
    }
  }
}
