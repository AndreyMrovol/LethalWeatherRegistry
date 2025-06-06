using HarmonyLib;

namespace WeatherRegistry.Compatibility
{
  internal class ImperiumCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }

      Plugin.debugLogger.LogInfo(ModGUID + " detected! Patching Imperium.");

      Plugin.harmony.Patch(
        AccessTools.Method("Imperium.Core.Lifecycle.MoonManager:RefreshWeatherEffects"),
        prefix: new HarmonyMethod(
          typeof(Patches.ImperiumPatches.ImperiumMoonManagerPatches),
          nameof(Patches.ImperiumPatches.ImperiumMoonManagerPatches.RefreshWeatherEffectsPrefixPatch)
        )
      );

      Plugin.harmony.Patch(
        AccessTools.Method("Imperium.Core.Lifecycle.PlayerManager:OnTeleportPlayerClient"),
        prefix: new HarmonyMethod(
          typeof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch),
          nameof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch.TeleportPlayerPrefixPatch)
        )
      );

      Plugin.harmony.Patch(
        AccessTools.Method("Imperium.Core.Lifecycle.PlayerManager:OnTeleportPlayerClient"),
        postfix: new HarmonyMethod(
          typeof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch),
          nameof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch.TeleportPlayerPostfixPatch)
        )
      );

      Plugin.harmony.Patch(
        AccessTools.Method("Imperium.Core.Lifecycle.PlayerManager:OnTeleportPlayerClient"),
        transpiler: new HarmonyMethod(
          typeof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch),
          nameof(Patches.ImperiumPatches.ImperiumPlayerManagerPatch.TeleportPlayerTranspilerPatch)
        )
      );
    }
  }
}
