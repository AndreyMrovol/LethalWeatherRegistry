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
        Plugin.logger.LogInfo("ButteryFixes detected, using ButteryFixes transpiler");
        return SpawnHiveButteryFixesTranspiler(instructions);
      }

      var matcher = new CodeMatcher(instructions).MatchForward(
        false,
        new CodeMatch(OpCodes.Ldarg_0),
        new CodeMatch(OpCodes.Ldloc_2), // gameObject
        new CodeMatch(OpCodes.Callvirt),
        new CodeMatch(OpCodes.Call),
        new CodeMatch(OpCodes.Ldloc_3) // num
      );

      if (matcher.IsInvalid)
      {
        Plugin.logger.LogError("Failed to find SpawnHiveClientRpc call in SpawnHiveNearEnemy!");
        return instructions;
      }

      // Advance to after ldloc_3 instruction
      matcher.Advance(5);

      // Insert our adjustment call which will consume the int from the stack and push the adjusted value
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
