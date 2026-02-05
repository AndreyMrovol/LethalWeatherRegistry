using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class EventManager
  {
    public static WeatherRegistryEvent DisableAllWeathers = new();

    public static WeatherRegistryEvent BeforeSetupStart = new();
    public static WeatherRegistryEvent SetupFinished = new();
    public static WeatherRegistryEvent<int> DayChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, Weather weather, string screenText)> MapScreenUpdated = new();
    public static WeatherRegistryEvent<(SelectableLevel level, Weather weather)> WeatherChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, Weather weather)> ShipLanding = new();
  }
}
