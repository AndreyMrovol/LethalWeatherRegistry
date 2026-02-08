using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using WeatherRegistry.Enums;

namespace WeatherRegistry.Patches
{
  public class CodeRebirthPatches
  {
    public static void Init()
    {
      Plugin.harmony.Patch(
        original: AccessTools.Method("CodeRebirth.src.MiscScripts.WeatherController:Start"),
        prefix: new HarmonyMethod(typeof(CodeRebirthPatches).GetMethod(nameof(StartPrefixPatch)))
      );
      Plugin.logger.LogMessage("Patched CodeRebirth.src.MiscScripts.WeatherController:Start");

      Plugin.harmony.Patch(
        original: AccessTools.Method("CodeRebirth.src.Util.CodeRebirthUtils:OnNetworkSpawn"),
        transpiler: new HarmonyMethod(typeof(CodeRebirthPatches).GetMethod(nameof(CRUtils_OnNetworkSpawn)))
      );
      Plugin.logger.LogMessage("Patched CodeRebirth.src.Util.CodeRebirthUtils:OnNetworkSpawn");
    }

    public static bool StartPrefixPatch(CodeRebirth.src.MiscScripts.WeatherController __instance)
    {
      bool IsClearWeather = WeatherManager.GetCurrentLevelWeather().Type == WeatherType.Clear;

      CodeRebirth.src.Plugin.ExtendedLogging($"Weather: {WeatherManager.GetCurrentLevelWeather().Type}, IsClearWeather: {IsClearWeather}");

      if (!IsClearWeather)
      {
        __instance.StartCoroutine(__instance.HandleDarkness());
      }

      return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static IEnumerable<CodeInstruction> CRUtils_OnNetworkSpawn(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher matcher = new(instructions);

      matcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Ldarg_0), // Load 'this'
        new CodeMatch(OpCodes.Ldarg_0), // Load 'this' again
        new CodeMatch(OpCodes.Ldloc_0), // Load moonInfo local variable
        new CodeMatch(OpCodes.Call, AccessTools.Method("CodeRebirth.src.Util.CodeRebirthUtils:HandleWRSetupWithOxyde")),
        new CodeMatch(
          OpCodes.Call,
          AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(IEnumerator) })
        ),
        new CodeMatch(OpCodes.Pop)
      );

      if (!matcher.IsValid)
      {
        Plugin.logger.LogError("CodeRebirth transpiler failed to find StartCoroutine pattern!");
        return instructions;
      }

      // Remove the original 6 instructions
      matcher.RemoveInstructions(6);

      // Call to HandleWRSetupWithOxyde method with SelectableLevel parameter
      matcher.Insert(
        new CodeInstruction(OpCodes.Ldloc_0), // Load moonInfo
        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(AccessTools.TypeByName("Dawn.DawnMoonInfo"), "Level")), // Get moonInfo.Level
        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CodeRebirthPatches), nameof(HandleWRSetupWithOxyde))),
        new CodeInstruction(OpCodes.Pop) // Pop the IEnumerator return value
      );

      return matcher.InstructionEnumeration();
    }

    private static IEnumerator HandleWRSetupWithOxyde(SelectableLevel level)
    {
      yield return new WaitUntil(() => WeatherRegistry.WeatherManager.IsSetupFinished);

      if (TimeOfDay.Instance.daysUntilDeadline <= 0)
      {
        WeatherRegistry.WeatherController.ChangeWeather(level, LevelWeatherType.None);
        yield break;
      }

      if (
        !CodeRebirth.src.Plugin.ModConfig.ConfigOxydeNeedsNightShift.Value
        && WeatherRegistry.WeatherManager.GetCurrentWeather(level).Type != WeatherType.Clear
      )
        yield break;

      CodeRebirth.src.Plugin.ExtendedLogging(
        $"Switch weather to: {Dawn.LethalContent.Weathers[CodeRebirth.CodeRebirthWeatherKeys.NightShift].WeatherEffect.name}"
      );
      CodeRebirth.src.Plugin.ExtendedLogging(
        $"LevelweatherType: {(LevelWeatherType)TimeOfDay.Instance.effects.IndexOf(Dawn.LethalContent.Weathers[CodeRebirth.CodeRebirthWeatherKeys.NightShift].WeatherEffect)}"
      );
      WeatherRegistry.WeatherController.ChangeWeather(
        level,
        (LevelWeatherType)
          TimeOfDay.Instance.effects.IndexOf(Dawn.LethalContent.Weathers[CodeRebirth.CodeRebirthWeatherKeys.NightShift].WeatherEffect)
      );
    }
  }
}
