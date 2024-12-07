using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace WeatherRegistry.Compatibility
{
  internal class LobbyControlCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }

      // manually patch namespace LobbyControl.TerminalCommands private static bool ClearCommand

      Plugin.harmony.Patch(
        AccessTools.Method("LobbyControl.TerminalCommands.LobbyCommand:ClearCommand"),
        postfix: new HarmonyMethod(typeof(LobbyControlCompat), nameof(LobbyControlClearCommand))
      );

      Plugin.debugLogger.LogInfo("Patched LobbyControl.TerminalCommands.LobbyCommand:ClearCommand");
    }

    private static void LobbyControlClearCommand()
    {
      WeatherEffect[] effects = TimeOfDay.Instance.effects;
      List<WeatherEffect> effectList = effects.ToList();

      // Remove all weather effects past the vanilla indexes
      int lowestModdedIndex;

      try
      {
        lowestModdedIndex = WeatherManager.ModdedWeatherEnumExtension.Keys.Min();
      }
      catch
      {
        Plugin.logger.LogWarning("No modded weather effects found");
        return;
      }

      Plugin.debugLogger.LogInfo($"Lowest modded index: {lowestModdedIndex}");
      for (int i = lowestModdedIndex; i < effects.Length; i++)
      {
        WeatherEffect effect = effects[i];
        if (effect != null)
        {
          effectList.Remove(effect);
        }
      }

      TimeOfDay.Instance.effects = effectList.ToArray();
    }
  }
}
