using System.Numerics;
using HarmonyLib;
using WeatherRegistry.Patches;

namespace WeatherRegistry.Compatibility
{
  internal class ButteryFixesCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }
      Plugin.debugLogger.LogInfo("ButteryFixes mod detected - enabling compatibility features");

      Plugin.harmony.Patch(
        original: AccessTools.Method("ButteryFixes.Utility.NonPatchFunctions:RerollHivePrice"),
        postfix: new HarmonyLib.HarmonyMethod(typeof(ButteryFixesCompat).GetMethod(nameof(RerollHivePricePatch)))
      );
    }

    public static int RerollHivePricePatch(int price, Vector3 pos)
    {
      return RedLocustBeesSpawnPatch.GetAdjustedHiveValue(price);
    }
  }
}
