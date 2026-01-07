using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using WeatherRegistry.Enums;
using WeatherRegistry.Modules;
using LoggingType = MrovLib.LoggingType;

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

    // algorithm settings
    internal static ConfigEntry<WeatherAlgorithm> WeatherAlgorithm { get; private set; }
    internal static ConfigEntry<bool> FirstDayClear { get; private set; }

    // map screen settings
    internal static ConfigEntry<bool> ColoredWeathers { get; private set; }
    internal static ConfigEntry<bool> PlanetVideos { get; private set; }
    internal static ConfigEntry<bool> ShowWeatherMultipliers { get; private set; }

    // module toggles
    internal static ConfigEntry<bool> UseScrapMultipliers { get; private set; }

    private ConfigManager(ConfigFile config)
    {
      configFile = config;

      LoggingLevels = configFile.Bind("|Logging", "Display Log Levels", LoggingType.Basic, "Select which logs to show.");

      WeatherAlgorithm = configFile.Bind(
        "|WeatherSelection",
        "Weather Selection Algorithm",
        Enums.WeatherAlgorithm.Registry,
        "Select the algorithm to use during weather selection."
      );
      FirstDayClear = configFile.Bind(
        "|WeatherSelection",
        "First Day Clear Weather",
        false,
        "If enabled, the first day will always have clear weather, on all planets, regardless of the selected algorithm."
      );

      ColoredWeathers = configFile.Bind("|General", "Colored Weathers", true, "Enable colored weathers on map screen");
      PlanetVideos = configFile.Bind("|General", "Planet Videos", true, "Display planet videos on map screen");
      ShowWeatherMultipliers = configFile.Bind("|General", "Show Weather Multipliers", false, "Show weather multipliers on map screen");

      UseScrapMultipliers = configFile.Bind(
        "|General",
        "Scrap multipliers",
        true,
        "Use Registry's scrap multipliers. Disable if you prefer to use other mod's multiplier settings."
      );
    }

    // TODO: add change listeners to allow hot-reloading algorithm/weights/filters

    internal void RemoveOrphanedEntries()
    {
      //remove orphaned config entries
      Plugin.logger.LogInfo("Removing orphaned config entries...");
      var orphanedEntriesProp = ConfigManager
        .configFile.GetType()
        .GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
      var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp!.GetValue(ConfigManager.configFile, null);

      var entriesToRemove = orphanedEntries.Where(entry => entry.Key.Section.Contains("|")).ToList();

      Plugin.logger.LogWarning($"Found {entriesToRemove.Count} orphaned config entries.");

      entriesToRemove.ForEach(entry =>
      {
        Plugin.debugLogger.LogWarning($"Removing orphaned config entry: {entry.Key.Section} - {entry.Key.Key}");
        orphanedEntries.Remove(entry.Key);
      });

      ConfigManager.configFile.Save();
    }

    internal static void ReloadConfigfile()
    {
      Plugin.logger.LogInfo("Reloading config file...");
      configFile.Reload();
    }

    internal static void SettingChanged(object sender, SettingChangedEventArgs args)
    {
      ConfigEntryBase changedEntry = args.ChangedSetting;

      Plugin.logger.LogInfo(
        $"Setting changed: {changedEntry.Definition.Section}/{changedEntry.Definition.Key} changed to {changedEntry.BoxedValue}"
      );
    }
  }
}
