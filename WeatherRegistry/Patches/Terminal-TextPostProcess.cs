using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  internal class TerminalPostprocessPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Terminal), "TextPostProcess", MethodType.Normal)]
    static IEnumerable<CodeInstruction> MoonCatalogueListTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      MethodInfo getWeatherStringMethod = AccessTools.Method(typeof(TerminalPostprocessPatch), "GetPlanetWeatherDisplayString");

      CodeMatcher matcher = new(instructions);

      // 70	00D0	ldloc.2
      // 71	00D1	ldarg.1
      // 72	00D2	ldloc.1
      // 73	00D3	ldc.i4.1
      // 74	00D4	callvirt	instance string [netstandard]System.Text.RegularExpressions.Regex::Replace(string, string, int32)

      matcher
        .MatchForward(
          false,
          new CodeMatch(OpCodes.Ldloc_2),
          new CodeMatch(OpCodes.Ldarg_1),
          new CodeMatch(OpCodes.Ldloc_1),
          new CodeMatch(OpCodes.Ldc_I4_1),
          new CodeMatch(OpCodes.Callvirt)
        )
        .Advance(2) // Move to the Ldloc_1 instruction
        .RemoveInstruction()
        .Insert(
          new CodeInstruction(OpCodes.Ldarg_0), // this
          new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Terminal), "moonsCatalogueList")),
          new CodeInstruction(OpCodes.Ldloc_3), // num2
          new CodeInstruction(OpCodes.Ldelem_Ref), // Get the SelectableLevel
          new CodeInstruction(OpCodes.Ldc_I4_1), // true for parentheses parameter
          new CodeInstruction(OpCodes.Call, getWeatherStringMethod)
        );

      return matcher.InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Terminal), "TextPostProcess", MethodType.Normal)]
    static IEnumerable<CodeInstruction> CurrentPlanetTimeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
      MethodInfo getCurrentPlanetTimeMethod = AccessTools.Method(typeof(TerminalPostprocessPatch), "GetCurrentPlanetTimeText");
      CodeMatcher matcher = new(instructions);

      // 113	0141	ldarg.1
      // 114	0142	ldstr	"[currentPlanetTime]"
      // 115	0147	ldloc.1
      // 116	0148	callvirt	instance string [netstandard]System.String::Replace(string, string)

      matcher
        // Find the String.Replace call
        .MatchForward(
          false,
          new CodeMatch(OpCodes.Ldarg_1),
          new CodeMatch(OpCodes.Ldstr, "[currentPlanetTime]"),
          new CodeMatch(OpCodes.Ldloc_1), // text (this is what we want to replace)
          new CodeMatch(OpCodes.Callvirt) // String.Replace call
        )
        // Move to where 'text' is loaded
        .Advance(2)
        // Replace the loading of 'text' with our custom method call
        .RemoveInstruction()
        .Insert(
          // Load the node.displayPlanetInfo value
          new CodeInstruction(OpCodes.Ldarg_2), // Node is in argument 2
          new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerminalNode), "displayPlanetInfo")),
          new CodeInstruction(OpCodes.Call, getCurrentPlanetTimeMethod)
        );

      return matcher.InstructionEnumeration();
    }

    private static string GetPlanetWeatherDisplayString(SelectableLevel level, bool parentheses = false)
    {
      Plugin.debugLogger.LogDebug($"GetPlanetWeatherDisplayString called for {level.PlanetName}");
      string overrideString = WeatherManager.WeatherDisplayOverride(level);

      return overrideString == string.Empty
        ? $"{(parentheses ? "(" : "")}{WeatherManager.GetWeather(level.currentWeather).Name}{(parentheses ? ")" : "")}"
        : $"{(parentheses ? "(" : "")}{overrideString}{(parentheses ? ")" : "")}";
    }

    private static string GetCurrentPlanetTimeText(int displayPlanetInfo)
    {
      SelectableLevel level = StartOfRound.Instance.levels[displayPlanetInfo];
      Plugin.debugLogger.LogDebug($"GetCurrentPlanetTimeText called for {level.PlanetName}");

      return GetPlanetWeatherDisplayString(level, false).ToLower();
    }

    [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
    [HarmonyPrefix]
    internal static void Prefix(ref TerminalNode node, ref string modifiedDisplayText)
    {
      if (node.displayPlanetInfo != -1)
      {
        Regex regex = new(@"\ It is (\n)");
        node.displayText = regex.Replace(node.displayText, " It is ");
        modifiedDisplayText = regex.Replace(modifiedDisplayText, " It is ");
      }
    }
  }
}
