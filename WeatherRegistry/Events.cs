using WeatherRegistry.Events;

namespace WeatherRegistry
{
  public class EventManager
  {
    public static WeatherRegistryEvent DisableAllWeathers = new();

    public static WeatherRegistryEvent BeforeSetupStart = new();
    public static WeatherRegistryEvent SetupFinished = new();
    public static WeatherRegistryEvent<int> DayChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, ImprovedWeather weather, string screenText)> MapScreenUpdated = new();
    public static WeatherRegistryEvent<(SelectableLevel level, ImprovedWeather weather)> WeatherChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, ImprovedWeather weather)> ShipLanding = new();
  }
}

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace WeatherRegistry.Events
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
  public class WeatherRegistryEvent<T> : MrovLib.Events.CustomEvent<T> { }

  public class WeatherRegistryEvent : MrovLib.Events.CustomEvent { }
}
