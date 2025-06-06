using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Imperium.API.Types.Networking;

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
