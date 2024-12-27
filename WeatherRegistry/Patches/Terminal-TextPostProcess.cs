using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using WeatherRegistry.Definitions;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  [HarmonyPatch(typeof(Terminal))]
  internal class TerminalPostprocessPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Terminal), "TextPostProcess", MethodType.Normal)]
    internal static IEnumerable<CodeInstruction> Terminal_textPostProcess(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher matcher = new(instructions);

      // 64	00B5	ldflda	valuetype LevelWeatherType SelectableLevel::currentWeather
      // 65	00BA	constrained.	LevelWeatherType
      // 66	00C0	callvirt	instance string [netstandard]System.Object::ToString()

      CodeMatcher matcher2 = matcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(SelectableLevel), "currentWeather")),
        new CodeMatch(OpCodes.Constrained, AccessTools.TypeByName("LevelWeatherType")),
        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(object), "ToString"))
      );

      // replace the callvirt with a call to our custom method
      matcher2.RemoveInstructions(3);
      matcher2.InsertAndAdvance(
        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TerminalPostprocessPatch), "GetPlanetWeatherDisplayString"))
      );

      // 104	0122	ldflda	valuetype LevelWeatherType SelectableLevel::currentWeather
      // 105	0127	constrained.	LevelWeatherType
      // 106	012D	callvirt	instance string [netstandard]System.Object::ToString()
      // 107	0132	callvirt	instance string [netstandard]System.String::ToLower()

      CodeMatcher matcher3 = matcher2.MatchForward(
        false,
        new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(SelectableLevel), "currentWeather")),
        new CodeMatch(OpCodes.Constrained, AccessTools.TypeByName("LevelWeatherType")),
        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(object), "ToString")),
        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(string), "ToLower"))
      );

      // replace the callvirt with a call to our custom method
      matcher3.RemoveInstructions(4);
      matcher3.InsertAndAdvance(
        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TerminalPostprocessPatch), "GetPlanetWeatherDisplayString")),
        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(string), "ToLower"))
      );

      return matcher.InstructionEnumeration();
    }

    private static string GetPlanetWeatherDisplayString(SelectableLevel level)
    {
      Plugin.logger.LogDebug($"GetPlanetWeatherDisplayString called for {level.PlanetName}");
      string overrideString = WeatherManager.WeatherDisplayOverride(level);

      return overrideString == string.Empty ? WeatherManager.GetWeather(level.currentWeather).Name : overrideString;
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
