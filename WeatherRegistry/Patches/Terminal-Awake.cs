using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TerminalAwakePatch
  {
    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    [HarmonyPriority(Priority.First)]
    public static bool TerminalPrefix(Terminal __instance)
    {
      if (WeatherManager.IsSetupFinished)
      {
        WeatherManager.IsSetupFinished = false;
      }

      return true;
    }
  }
}
