using System.Collections.Generic;
using WeatherRegistry.Events;
using WeatherRegistry.Modules;

namespace WeatherRegistry
{
  public class EventManager
  {
    public static WeatherRegistryEvent DisableAllWeathers = new();

    public static WeatherRegistryEvent SetupFinished = new();
    public static WeatherRegistryEvent<int> DayChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, RegistryWeather weather, string screenText)> MapScreenUpdated = new();
    public static WeatherRegistryEvent<(SelectableLevel level, RegistryWeather weather)> WeatherChanged = new();

    public static WeatherRegistryEvent<(SelectableLevel level, RegistryWeather weather)> ShipLanding = new();
  }
}
