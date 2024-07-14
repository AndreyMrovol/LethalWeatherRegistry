using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WeatherRegistry;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetMapScreenInfoToCurrentLevelPatch
  {
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    internal static void GameMethodPatch(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
      if (!WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("WeatherManager is not set up yet.");
        return;
      }

      StringBuilder stringBuilder = new();
      stringBuilder.Append("ORBITING: " + ___currentLevel.PlanetName + "\n");
      stringBuilder.Append($"WEATHER: {GetColoredString(___currentLevel)}\n");
      stringBuilder.Append(___currentLevel.LevelDescription ?? "");

      ___screenLevelDescription.fontWeight = FontWeight.Bold;
      ___screenLevelDescription.text = stringBuilder.ToString();

      EventManager.MapScreenUpdated.Invoke(
        (level: ___currentLevel, weather: WeatherManager.GetCurrentWeather(___currentLevel), screenText: stringBuilder.ToString())
      );
    }

    private static string GetDisplayWeatherString(SelectableLevel level, Weather weather)
    {
      return weather.Name;
    }

    private static string GetColoredString(SelectableLevel level)
    {
      Weather currentWeather = WeatherManager.GetCurrentWeather(level);
      string currentWeatherString = GetDisplayWeatherString(level, currentWeather);

      if (!ConfigManager.ColoredWeathers.Value)
      {
        return currentWeatherString;
      }

      string outputString = "";
      Regex split = new(@"(\/)|(\?)|(>)|(\+)");

      split
        .Split(currentWeatherString)
        .ToList()
        .ForEach(word =>
        {
          string newWord = word.Trim();
          string pickedColor;

          // resolve weather name string into color using Settings.ScreenMapColors dictionary
          // so other mods (like weathertweaks) can add their own colors and symbols
          pickedColor = ColorUtility.ToHtmlStringRGB(Settings.ScreenMapColors.TryGetValue(newWord, out Color value) ? value : Color.black);

          outputString += pickedColor != "000000" ? $"<color=#{pickedColor}>{word}</color>" : $"{newWord}";
        });

      return outputString;
    }
  }
}
