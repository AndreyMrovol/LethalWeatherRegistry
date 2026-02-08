namespace WeatherRegistry.Compatibility
{
  internal class CRCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }

      if (this.ModVersion != "1.4.1")
      {
        Plugin.debugLogger.LogWarning($"CodeRebirth version {this.ModVersion} detected, but version 1.4.1 is required for Registry patches.");
        return;
      }

      Plugin.debugLogger.LogInfo("CodeRebirth detected! Patching CodeRebirth to work with WR v0.8+");

      Patches.CodeRebirthPatches.Init();
    }
  }
}
