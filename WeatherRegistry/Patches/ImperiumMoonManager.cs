namespace WeatherRegistry.Patches
{
  public static partial class ImperiumPatches
  {
    public static class ImperiumMoonManagerPatches
    {
      public static bool RefreshWeatherEffectsPrefixPatch()
      {
        return false;
      }
    }
  }
}
