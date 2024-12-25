using WeatherRegistry.Modules;

namespace WeatherRegistry.Modules
{
  public class WeatherModExtendedConfig : RegistryWeatherConfig
  {
    // Implementing this class lets you add custom configuration options to your weather type
    // and including those options in `mrov.WeatherRegistry.cfg` config file right next to other options.

    // if you replace Weather.Config with this, you can add custom configuration options to your weather type
    // since it's an extension type of RegistryWeatherConfig

    // There are config handlers for different types of values:
    // - IntegerConfigHandler for integer values
    // - FloatConfigHandler for float values
    // - BooleanConfigHandler for boolean values
    // - LevelListConfigHandler for level lists
    // - LevelWeightsConfigHandler for level weights
    // - WeatherWeightsConfigHandler for weather weights
    // check https://github.com/AndreyMrovol/LethalWeatherRegistry/blob/142e87aa00c26139a1c273273193ff9ee01d0d30/WeatherRegistry/ConfigHelper.cs#L35-L169 for more

    // You can also create your own config handlers for your own desired value types
    // by inheriting from ConfigHandler<T, CT> and implementing the necessary methods.

    // i am proud of what's cooked in here

    // this is an example config entry that enables or disables the weather type
    public BooleanConfigHandler EnableWeather = new(true, true);

    public override void Init(Weather weather)
    {
      EnableWeather.SetConfigEntry(weather, "Enable Weather", new("Enable this weather type"));

      // this is called to initialize the rest of the config entries in WeatherRegistry's class
      base.Init(weather);
    }
  }
}
