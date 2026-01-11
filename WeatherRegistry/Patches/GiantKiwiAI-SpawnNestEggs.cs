using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(GiantKiwiAI))]
  public static class GiantKiwiAISpawnPatch
  {
    [HarmonyPatch("SpawnNestEggs")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SpawnNestEggsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      var matcher = new CodeMatcher(instructions).MatchForward(
        false,
        new CodeMatch(OpCodes.Ldarg_0),
        new CodeMatch(OpCodes.Ldloc_1),
        new CodeMatch(OpCodes.Ldloc_2),
        new CodeMatch(OpCodes.Ldloc_S)
      );

      if (matcher.IsInvalid)
      {
        Plugin.logger.LogError("Failed to find SpawnEggsClientRpc call in SpawnNestEggs!");
        return instructions;
      }

      // Insert our adjustment code before the SpawnEggsClientRpc call
      matcher.Insert(
        new CodeInstruction(OpCodes.Ldloc_2), // Load array2 (egg values)
        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GiantKiwiAISpawnPatch), nameof(AdjustEggValuesForWeather)))
      );

      Plugin.logger.LogInfo("Successfully patched SpawnNestEggs with weather multiplier adjustment!");
      return matcher.InstructionEnumeration();
    }

    private static void AdjustEggValuesForWeather(int[] eggScrapValues)
    {
      Weather currentWeather = WeatherManager.GetCurrentLevelWeather();

      Plugin.logger.LogDebug($"Adjusting {eggScrapValues.Length} eggs with weather multiplier {currentWeather.ScrapValueMultiplier}");

      for (int i = 0; i < eggScrapValues.Length; i++)
      {
        int originalValue = eggScrapValues[i];
        eggScrapValues[i] = Mathf.CeilToInt(eggScrapValues[i] * currentWeather.ScrapValueMultiplier);
        Plugin.logger.LogDebug($"Egg {i}: {originalValue} -> {eggScrapValues[i]}");
      }
    }
  }
}
