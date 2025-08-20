using System;
using System.Collections.Generic;
using MrovLib;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public static class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    internal static Logger Logger = new("WeatherCalculation", LoggingType.Basic);

    public enum WeatherAlgorithm
    {
      Registry,
      Vanilla,
      Hybrid,
    }

    internal static WeatherSelectionAlgorithm RegistryAlgorithm = new Algorithms.WeatherRegistryWeatherSelection();
    internal static WeatherSelectionAlgorithm VanillaAlgorithm = new Algorithms.VanillaWeatherSelection();
    internal static WeatherSelectionAlgorithm HybridAlgorithm = new Algorithms.HybridWeatherSelection();

    public static Dictionary<WeatherAlgorithm, WeatherSelectionAlgorithm> WeatherAlgorithms =
      new()
      {
        { WeatherAlgorithm.Registry, RegistryAlgorithm },
        { WeatherAlgorithm.Vanilla, VanillaAlgorithm },
        { WeatherAlgorithm.Hybrid, HybridAlgorithm },
      };

    [Obsolete("Use Settings.WeatherSelectionAlgorithm instead")]
    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm
    {
      get { return Settings.WeatherSelectionAlgorithm; }
      set { Settings.WeatherSelectionAlgorithm = value; }
    }
  }
}
