using System.Collections.Generic;
using Unity;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class Settings
  {
    public static Dictionary<string, Color> ScreenMapColors = [];

    public static bool SelectWeathers { get; set; } = true;

    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm { get; set; } =
      ConfigManager.UseWeatherWeights.Value ? WeatherCalculation.RegistryAlgorithm : WeatherCalculation.VanillaAlgorithm;

    public static bool ScrapMultipliers { get; set; } = ConfigManager.UseScrapMultipliers.Value;
  }
}
