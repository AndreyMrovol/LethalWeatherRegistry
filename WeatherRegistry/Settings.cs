using System.Collections.Generic;
using Unity;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry
{
  public class Settings
  {
    public static bool SetupFinished => WeatherManager.IsSetupFinished;

    public static bool IsGameStarted { get; internal set; } = false;

    public static bool IsPlayerInside { get; set; } = false;

    public static Dictionary<string, Color> ScreenMapColors = [];

    public static bool SelectWeathers { get; set; } = true;

    public static WeatherSelectionAlgorithm WeatherSelectionAlgorithm { get; set; } =
      ConfigManager.UseWeatherWeights.Value ? WeatherCalculation.RegistryAlgorithm : WeatherCalculation.VanillaAlgorithm;

    public static bool ScrapMultipliers { get; set; } = ConfigManager.UseScrapMultipliers.Value;

    public static bool ColoredWeathers { get; set; } = ConfigManager.ColoredWeathers.Value;
    public static bool PlanetVideos { get; set; } = ConfigManager.PlanetVideos.Value;
    public static bool MapScreenOverride { get; set; } = true;
  }
}
