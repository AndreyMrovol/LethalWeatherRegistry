using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace WeatherRegistry
{
  public class ConfigManager
  {
    public static ConfigManager Instance { get; private set; }
    internal static ConfigFile configFile;

    public static void Init(ConfigFile config)
    {
      Instance = new ConfigManager(config);
    }

    // general settings

    // map screen settings
    public static ConfigEntry<bool> ColoredWeathers { get; private set; }

    // sun animator settings
    private static ConfigEntry<string> SunAnimatorBlacklist { get; set; }
    public static SelectableLevel[] SunAnimatorBlacklistLevels { get; internal set; }

    private ConfigManager(ConfigFile config)
    {
      configFile = config;

      EventManager.setupFinished.AddListener(StartupActions);

      ColoredWeathers = configFile.Bind("|General", "Colored Weathers", true, "Enable colored weathers in map screen");
      SunAnimatorBlacklist = configFile.Bind(
        "|SunAnimator",
        "Blacklist",
        "Asteroid-13;",
        "Semicolon-separated list of level names to blacklist from being patched by sun animator"
      );
    }

    private static void OnConfigChange(object sender, EventArgs eventArgs)
    {
      SunAnimatorBlacklistLevels = ConfigHelper.ConvertStringToLevels(SunAnimatorBlacklist.Value);
    }

    internal static void StartupActions()
    {
      SunAnimatorBlacklistLevels = ConfigHelper.ConvertStringToLevels(SunAnimatorBlacklist.Value);
      SunAnimatorBlacklist.SettingChanged += OnConfigChange;
    }
  }
}
