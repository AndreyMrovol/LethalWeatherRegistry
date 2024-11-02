using BepInEx.Configuration;

namespace WeatherRegistry.Definitions
{
  public abstract class ConfigHandler<T, CT> : MrovLib.ConfigHandler<T, CT>
  {
    public ConfigDescription _configDescription;
    public bool _enabled = true;

    public bool ConfigEntryActive => (ConfigEntry != null) && Enabled;

    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        _enabled = value;

        if (ConfigEntry != null)
        {
          if (!Enabled)
          {
            _configDescription = new ConfigDescription(
              $"{(Enabled ? "" : "**This setting has been disabled by the mod developer and won't take any effect.**\n")}{_configDescription.Description}",
              _configDescription.AcceptableValues
            );
          }

          var NewConfigEntry = ConfigEntry;

          ConfigManager.configFile.Remove(ConfigEntry.Definition);
          ConfigEntry = ConfigManager.configFile.Bind(
            NewConfigEntry.Definition.Section,
            NewConfigEntry.Definition.Key,
            DefaultValue,
            _configDescription
          );
        }
      }
    }
  }
}
