using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetMapScreenInfoToCurrentLevelPatch
  {
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    internal static void GameMethodPatch(
      ref TextMeshProUGUI ___screenLevelDescription,
      ref SelectableLevel ___currentLevel,
      StartOfRound __instance
    )
    {
      if (!WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("WeatherManager is not set up yet.");
        return;
      }

      if (!Settings.MapScreenOverride)
      {
        return;
      }

      Regex multiNewLine = new(@"\n{2,}");

      StringBuilder stringBuilder = new();
      stringBuilder.Append($"ORBITING: {___currentLevel.PlanetName}\n");
      stringBuilder.Append($"WEATHER: {GetColoredString(___currentLevel)}\n");
      stringBuilder.Append(multiNewLine.Replace(___currentLevel.LevelDescription, "\n") ?? "");

      ___screenLevelDescription.fontWeight = FontWeight.Bold;
      ___screenLevelDescription.SetText(stringBuilder.ToString());

      __instance.screenLevelVideoReel.enabled = Settings.PlanetVideos;

      EventManager.MapScreenUpdated.Invoke(
        (level: ___currentLevel, weather: WeatherManager.GetCurrentWeather(___currentLevel), screenText: stringBuilder.ToString())
      );
    }

    // it's like that because of weathertweaks
    private static string GetDisplayWeatherString(SelectableLevel level, Weather weather)
    {
      return weather.Name;
    }

    internal static string GetColoredString(SelectableLevel level)
    {
      Weather currentWeather = WeatherManager.GetCurrentWeather(level);
      string currentWeatherString = GetDisplayWeatherString(level, currentWeather);

      if (!Settings.ColoredWeathers)
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
          string pickedColorValue;

          // resolve weather name string into color using Settings.ScreenMapColors dictionary
          // so other mods (like weathertweaks) can add their own colors and symbols

          UnityEngine.Color pickedColor = Settings.ScreenMapColors.TryGetValue(newWord, out Color value) ? value : Color.black;
          if (pickedColor != Color.black)
          {
            // add 10% of green value to make the editor color correctly display in-game (v70 screen color change)
            pickedColor = new Color(pickedColor.r, pickedColor.g * 1.1f, pickedColor.b, pickedColor.a);
            pickedColorValue = ColorUtility.ToHtmlStringRGB(pickedColor);
          }
          else
          {
            // if the color is not found, use black
            pickedColorValue = "000000";
          }

          outputString += pickedColorValue != "000000" ? $"<color=#{pickedColorValue}>{word}</color>" : $"{newWord}";
        });

      return outputString;
    }
  }
}
