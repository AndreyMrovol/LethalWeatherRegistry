using BepInEx.Configuration;

namespace WeatherRegistry.Utils
{
  public abstract class ConfigHandler<T, CT> : MrovLib.ConfigHandler<T, CT>
  {
    public ConfigDescription _configDescription;
    public bool _enabled = true;

    public bool ConfigEntryActive => (ConfigEntry != null) && Enabled;

    public bool Enabled { get; set; }
  }
}
