using System;

namespace WeatherRegistry
{
  internal class LobbyCompatibilityCompatibility
  {
    public static void Init()
    {
      Plugin.logger.LogDebug("LobbyCompatibility detected, registering plugin with LobbyCompatibility.");

      Version pluginVersion = Version.Parse(PluginInfo.PLUGIN_VERSION);

      LobbyCompatibility.Features.PluginHelper.RegisterPlugin(
        Plugin.GUID,
        pluginVersion,
        LobbyCompatibility.Enums.CompatibilityLevel.Everyone,
        LobbyCompatibility.Enums.VersionStrictness.None
      );
    }
  }
}
