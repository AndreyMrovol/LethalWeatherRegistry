using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  [HarmonyPatch(typeof(StartOfRound))]
  internal class OpeningDoorsSequencePatch
  {
    // to be completely honest, I have no idea what I'm doing
    // https://github.com/SylviBlossom/LC-SimpleWeatherDisplay/blob/2d252b92dcd4d8ef259b8072d9339ff5ccdc4d0b/src/Plugin.cs#L127-L153
    // this is just this, but repurposed

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
    internal static void StartOfRound_openingDoorsSequence(ILContext il)
    {
      var cursor = new ILCursor(il);

      if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<StartOfRound>("shipDoorsEnabled")))
      {
        Plugin.logger.LogError("Failed IL weather hook for StartOfRound.openingDoorsSequence");
        return;
      }
      else
      {
        Plugin.logger.LogInfo("IL weather hook for StartOfRound.openingDoorsSequence");
      }
      cursor.EmitDelegate<Action>(RunWeatherPatches);
      cursor.EmitDelegate<Action>(SetWeatherEffects);

      if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<SelectableLevel>("LevelDescription")))
      {
        Plugin.logger.LogError("Failed IL hook for StartOfRound.openingDoorsSequence");
        return;
      }

      cursor.Emit(OpCodes.Ldloc_1);
      cursor.EmitDelegate<Func<string, StartOfRound, string>>(
        (desc, self) =>
        {
          var weatherName =
            self.currentLevel.currentWeather != LevelWeatherType.None ? WeatherManager.GetCurrentWeatherName(self.currentLevel) : "Clear";
          var weatherLine = $"WEATHER: {weatherName}";

          return $"{weatherLine}\n{desc}";
        }
      );
    }

    internal static void RunWeatherPatches()
    {
      TimeOfDay.Instance.nextTimeSync = 0;
    }

    internal static void SetWeatherEffects()
    {
      Weather currentWeather = WeatherManager.GetCurrentWeather(StartOfRound.Instance.currentLevel);

      SunAnimator.OverrideSunAnimator(currentWeather.VanillaWeatherType);

      Plugin.logger.LogDebug(
        $"Landing at {ConfigHelper.GetNumberlessName(StartOfRound.Instance.currentLevel)} with weather {JsonConvert.SerializeObject(
        currentWeather,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      )}"
      );
    }
  }
}
