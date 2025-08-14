using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    internal static Logger Logger = new("WeatherCalculation", LoggingType.Basic);

    internal static WeatherSelectionAlgorithm RegistryAlgorithm = new Algorithms.WeatherRegistryWeatherSelection();
    internal static WeatherSelectionAlgorithm VanillaAlgorithm = new Algorithms.VanillaWeatherSelection();

    [Obsolete("Use Settings.WeatherSelectionAlgorithm instead")]
    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm
    {
      get { return Settings.WeatherSelectionAlgorithm; }
      set { Settings.WeatherSelectionAlgorithm = value; }
    }
  }
}
