using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(RedLocustBees))]
  public static class RedLocustBeesSpawnPatch
  {
    [HarmonyPatch("SpawnHiveNearEnemy")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SpawnHiveNearEnemyTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      if (Chainloader.PluginInfos.ContainsKey("butterystancakes.lethalcompany.butteryfixes"))
      {
        // i cannot get this motherfucking bullshit to work as a transpiler without breaking everything, i give up
        return instructions;
      }

      var matcher = new CodeMatcher(instructions).MatchForward(
        false,
        new CodeMatch(OpCodes.Ldloc_S),
        new CodeMatch(OpCodes.Ldloc_2),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), nameof(Vector3.up)))
      );

      if (matcher.IsInvalid)
      {
        Plugin.logger.LogError("Failed to find SpawnHiveClientRpc argument sequence in SpawnHiveNearEnemy!");
        return instructions;
      }

      matcher.Advance(1);
      matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RedLocustBeesSpawnPatch), nameof(GetAdjustedHiveValue))));

      Plugin.logger.LogDebug("Successfully patched SpawnHiveNearEnemy with weather multiplier adjustment!");
      return matcher.InstructionEnumeration();
    }

    public static IEnumerable<CodeInstruction> SpawnHiveButteryFixesTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      var matcher = new CodeMatcher(instructions).MatchForward(
        false,
        new CodeMatch(OpCodes.Ldloc_3), // num (hive value)
        new CodeMatch(OpCodes.Ldloc_1) // original instruction
      );

      if (matcher.IsInvalid)
      {
        Plugin.logger.LogError("Failed to find Ldloc_3 followed by Ldloc_1 in SpawnHiveNearEnemy (ButteryFixes variant)!");
        return instructions;
      }

      // Move past Ldloc_1, then skip ButteryFixes' inserted Ldloc_1 + Call instructions
      matcher.Advance(3);

      matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RedLocustBeesSpawnPatch), nameof(GetAdjustedHiveValue))));

      Plugin.logger.LogDebug("Successfully patched SpawnHiveNearEnemy with weather multiplier adjustment (ButteryFixes variant)!");
      return matcher.InstructionEnumeration();
    }

    internal static int GetAdjustedHiveValue(int originalValue)
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
