using System.Collections.Generic;
using WeatherRegistry.Events;

namespace WeatherRegistry
{
  public class EventManager
  {
    public static WeatherRegistryEvent DisableAllWeathers = new();

    public static WeatherRegistryEvent SetupFinished = new();
    public static WeatherRegistryEvent<(SelectableLevel level, Weather weather)> WeatherChanged = new();
    public static WeatherRegistryEvent<int> DayChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, Weather weather)> ShipLanding = new();
  }
}

namespace WeatherRegistry.Events
{
  public class WeatherRegistryEvent<T> : MrovLib.Events.CustomEvent<T> { }

  public class WeatherRegistryEvent : MrovLib.Events.CustomEvent { }
}
