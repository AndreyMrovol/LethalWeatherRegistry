using System;
using System.Collections.Generic;
using MrovLib;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;

namespace WeatherRegistry.Modules
{
  public static class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    internal static Logger Logger = new("WeatherCalculation", LoggingType.Basic);

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

    private static WeatherSelectionAlgorithm _weatherAlgorithm = WeatherAlgorithms[ConfigManager.WeatherAlgorithm.Value];
    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm
    {
      get { return _weatherAlgorithm; }
      set { _weatherAlgorithm = value; }
    }
  }
}
