using System.Collections.Generic;
using BepInEx.Configuration;

namespace WeatherAPI
{
  public class ConfigManager
  {
    public static ConfigManager Instance { get; private set; }
    internal readonly ConfigFile configFile;

    public static void Init(ConfigFile config)
    {
      Instance = new ConfigManager(config);
    }

    public static ConfigEntry<bool> ColoredWeathers { get; private set; }

    private ConfigManager(ConfigFile config)
    {
      configFile = config;

      ColoredWeathers = configFile.Bind("General", "Colored Weathers", true, "Enable colored weathers in map screen");
    }
  }
}
