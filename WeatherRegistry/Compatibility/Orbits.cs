namespace WeatherRegistry.Compatibility
{
  internal class OrbitsCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (this.IsModPresent)
      {
        Plugin.debugLogger.LogInfo("Orbits mod detected - enabling planet videos");
        Settings.PlanetVideos = true;
      }
    }
  }
}
