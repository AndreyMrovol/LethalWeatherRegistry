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
    }

    private static string GetColoredString(SelectableLevel level)
    {
      Weather currentWeather = WeatherManager.GetCurrentWeather(level);
      string currentWeatherString = currentWeather.Name;

      Plugin.logger.LogWarning($"Current weather: {currentWeatherString}");

      Color weatherColor = currentWeather.Color;
      string weatherColorString = ColorUtility.ToHtmlStringRGB(weatherColor);

      Plugin.logger.LogWarning($"Current weather color: {weatherColor} => {weatherColorString}");

      if (!ConfigManager.ColoredWeathers.Value)
      {
        return currentWeatherString;
      }

      // string outputString = currentWeatherString;
      string outputString = "";
      Regex split = new(@"(\/)|(\?)|(>)|(\+)");

      split
        .Split(currentWeatherString)
        .ToList()
        .ForEach(word =>
        {
          Plugin.logger.LogInfo(word);

          string newWord = word.Trim();
          Plugin.logger.LogDebug(newWord);

          string pickedColor;

          // resolve weather name string into color using Settings.ScreenMapColors dictionary
          // so other mods (like weathertweaks) can add their own colors and symbols
          pickedColor = ColorUtility.ToHtmlStringRGB(Settings.ScreenMapColors.TryGetValue(newWord, out Color value) ? value : Color.black);

          Plugin.logger.LogDebug(ColorUtility.ToHtmlStringRGB(Color.black));
          Plugin.logger.LogDebug(newWord);
          Plugin.logger.LogDebug(pickedColor);

          if (pickedColor != "000000")
          {
            pickedColor = weatherColorString;
          }

          outputString += pickedColor != "000000" ? $"<color=#{pickedColor}>{word}</color>" : $"{newWord}";
        });

      Plugin.logger.LogWarning($"Output string: {outputString}");

      return outputString;
    }
  }
}
