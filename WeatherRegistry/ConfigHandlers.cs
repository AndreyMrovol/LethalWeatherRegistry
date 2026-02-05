using System;
using BepInEx.Configuration;
using WeatherRegistry.Enums;

namespace WeatherRegistry
{
  public class Rarity
  {
    private int _weight;
    public int Weight
    {
      get { return _weight; }
      set { _weight = Math.Clamp(value, 0, 10000); }
    }
  }

  public class NameRarity : Rarity
  {
    public string Name { get; set; }
  }

  public class LevelRarity : Rarity
  {
    public SelectableLevel Level { get; set; }
  }

  public class WeatherRarity : Rarity
  {
    public Weather Weather { get; set; }
  }

  public abstract class ConfigHandler<T, CT> : Utils.ConfigHandler<T, CT>
  {
    public ConfigFile ConfigFile { get; set; } = ConfigManager.configFile;

    public ConfigHandler(CT value, bool enabled = true)
    {
      DefaultValue = value;
      Enabled = enabled;
    }

    public void SetConfigEntry(Weather weather, string configTitle, ConfigDescription configDescription = null)
    {
      if (Enabled)
      {
        ConfigEntry = ConfigFile.Bind(
          ConfigHelper.CleanStringForConfig(weather.ConfigCategory),
          ConfigHelper.CleanStringForConfig(configTitle),
          DefaultValue,
          configDescription
        );
      }
      else
      {
        ConfigEntry = null;
        Plugin.debugLogger.LogDebug($"Config entry for {weather.Name}: {configTitle} is disabled");
      }
    }

    public void SetConfigEntry(string configCategory, string configTitle, ConfigDescription configDescription = null)
    {
      if (Enabled)
      {
        ConfigEntry = ConfigFile.Bind(
          ConfigHelper.CleanStringForConfig(configCategory),
          ConfigHelper.CleanStringForConfig(configTitle),
          DefaultValue,
          configDescription
        );
      }
      else
      {
        ConfigEntry = null;
        Plugin.debugLogger.LogDebug($"Config entry {configTitle} is disabled");
      }
    }
  }

  public class LevelListConfigHandler : ConfigHandler<SelectableLevel[], string>
  {
    public LevelListConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public LevelListConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override SelectableLevel[] Value
    {
      get { return ConfigHelper.ConvertStringToLevels(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue); }
    }
  }

  public class LevelWeightsConfigHandler : ConfigHandler<LevelRarity[], string>
  {
    public LevelWeightsConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public LevelWeightsConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override LevelRarity[] Value
    {
      get { return ConfigHelper.ConvertStringToLevelRarities(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue); }
    }
  }

  public class WeatherWeightsConfigHandler : ConfigHandler<WeatherRarity[], string>
  {
    public WeatherWeightsConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public WeatherWeightsConfigHandler(string[] value, bool enabled = true)
      : base(String.Join(";", value), enabled) { }

    public override WeatherRarity[] Value
    {
      get { return ConfigHelper.ConvertStringToWeatherWeights(this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue); }
    }
  }

  public class BooleanConfigHandler : ConfigHandler<bool, bool>
  {
    public BooleanConfigHandler(bool value, bool enabled = true)
      : base(value, enabled) { }

    // this is done for the filtering option enum cause it's a bool config
    public BooleanConfigHandler(FilteringOption filteringOption, bool enabled = true)
      : base(filteringOption == FilteringOption.Include, enabled) { }

    public override bool Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class IntegerConfigHandler : ConfigHandler<int, int>
  {
    public IntegerConfigHandler(int value, bool enabled = true)
      : base(value, enabled) { }

    public override int Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class FloatConfigHandler : ConfigHandler<float, float>
  {
    public FloatConfigHandler(float value, bool enabled = true)
      : base(value, enabled) { }

    public override float Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }

  public class StringConfigHandler : ConfigHandler<string, string>
  {
    public StringConfigHandler(string value, bool enabled = true)
      : base(value, enabled) { }

    public override string Value
    {
      get { return this.ConfigEntryActive ? ConfigEntry.Value : this.DefaultValue; }
    }
  }
}
