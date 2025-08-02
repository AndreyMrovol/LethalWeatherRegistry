using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace WeatherRegistry
{
  internal class ConfigManager
  {
    internal static ConfigManager Instance { get; private set; }
    internal static ConfigFile configFile;

    internal static void Init(ConfigFile config)
    {
      Instance = new ConfigManager(config);
    }

    // general settings

    // logs settings
    internal static ConfigEntry<LoggingType> LoggingLevels { get; private set; }

    // map screen settings
    internal static ConfigEntry<bool> ColoredWeathers { get; private set; }
    internal static ConfigEntry<bool> PlanetVideos { get; private set; }
    internal static ConfigEntry<bool> ShowWeatherMultipliers { get; private set; }

    // module toggles
    internal static ConfigEntry<bool> UseWeatherWeights { get; private set; }
    internal static ConfigEntry<bool> UseScrapMultipliers { get; private set; }

    // sun animator settings
    internal static ConfigEntry<string> SunAnimatorBlacklist { get; set; }
    internal static SelectableLevel[] SunAnimatorBlacklistLevels { get; set; }

    private ConfigManager(ConfigFile config)
    {
      configFile = config;

      EventManager.SetupFinished.AddListener(StartupActions);

      LoggingLevels = configFile.Bind("|Logging", "Display Log Levels", LoggingType.Basic, "Select which logs to show.");

      ColoredWeathers = configFile.Bind("|General", "Colored Weathers", true, "Enable colored weathers on map screen");
      PlanetVideos = configFile.Bind("|General", "Planet Videos", true, "Display planet videos on map screen");
      ShowWeatherMultipliers = configFile.Bind("|General", "Show Weather Multipliers", false, "Show weather multipliers on map screen");

      UseWeatherWeights = configFile.Bind(
        "|General",
        "Weather weights",
        true,
        "Use weights for selecting weathers. Disable if you want to use vanilla algorithm."
      );
      UseScrapMultipliers = configFile.Bind(
        "|General",
        "Scrap multipliers",
        true,
        "Use Registry's scrap multipliers. Disable if you prefer to use other mod's multiplier settings."
      );

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
