using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace WeatherRegistry.Compatibility
{
  internal class LobbyControlCompat(string guid, string version = null) : MrovLib.CompatibilityHandler(guid, version)
  {
    public override void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }

      Plugin.harmony.Patch(
        AccessTools.Method("LobbyControl.TerminalCommands.LobbyCommand:ClearCommand"),
        postfix: new HarmonyMethod(typeof(LobbyControlCompat), nameof(LobbyControlReload))
      );

      Plugin.devLogger.LogInfo("Patched LobbyControl.TerminalCommands.LobbyCommand:ClearCommand");

      Plugin.harmony.Patch(
        AccessTools.Method("LobbyControl.TerminalCommands.LobbyCommand:LoadCommand"),
        postfix: new HarmonyMethod(typeof(LobbyControlCompat), nameof(LobbyControlReload))
      );

      Plugin.devLogger.LogInfo("Patched LobbyControl.TerminalCommands.LobbyCommand:LoadCommand");
    }

    private static void LobbyControlReload()
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

      Plugin.devLogger.LogInfo($"Lowest modded index: {lowestModdedIndex}");
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
