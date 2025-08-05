using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetMapScreenInfoToCurrentLevelPatch
  {
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyBefore("com.zealsprince.malfunctions")]
    internal static void GameMethodPatch(
      ref TextMeshProUGUI ___screenLevelDescription,
      ref SelectableLevel ___currentLevel,
      StartOfRound __instance
    )
    {
      Plugin.debugLogger.LogCustom($"SetMapScreenInfoToCurrentLevel called.", BepInEx.Logging.LogLevel.Debug, LoggingType.Developer);

      if (!WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("WeatherManager is not set up yet.");
        return;
      }

      if (Plugin.MalfunctionsCompat.IsModPresent && Plugin.MalfunctionsCompat.IsNavigationalMalfunctionActive())
      {
        Plugin.MalfunctionsCompat.SetNotifiedToFalse();
        Plugin.debugLogger.LogDebug("Navigation Malfunction is active, skipping map screen update.");
        return;
      }

      if (!Settings.MapScreenOverride)
      {
        return;
      }

      Regex multiNewLine = new(@"\n{2,}");
      string planetName = ___currentLevel.PlanetName;

      Weather currentWeather = WeatherManager.GetCurrentWeather(___currentLevel);
      if (WeatherOverrideManager.GetCurrentWeatherOverride(___currentLevel, currentWeather) is WeatherEffectOverride currentOverride)
      {
        string newName = WeatherOverrideManager.GetPlanetOverrideName(currentOverride);

        planetName = !string.IsNullOrEmpty(newName) ? $"{planetName} ({newName})" : ___currentLevel.PlanetName;
      }

      StringBuilder stringBuilder = new();

      stringBuilder.Append($"ORBITING: {ScaleDownName(planetName)}\n");
      stringBuilder.Append($"WEATHER: {ScaleDownName(GetColoredString(___currentLevel))}\n");

      if (ConfigManager.ShowWeatherMultipliers.Value)
      {
        stringBuilder.Append($"MULTIPLIERS: ＄x{currentWeather.ScrapValueMultiplier}  ▼x{currentWeather.ScrapAmountMultiplier}\n");
      }

      stringBuilder.Append(multiNewLine.Replace(___currentLevel.LevelDescription, "\n") ?? "");

      ___screenLevelDescription.fontWeight = FontWeight.Bold;
      ___screenLevelDescription.SetText(stringBuilder.ToString());

      __instance.screenLevelVideoReel.enabled = Settings.PlanetVideos;

      EventManager.MapScreenUpdated.Invoke(
        (level: ___currentLevel, weather: WeatherManager.GetCurrentWeather(___currentLevel), screenText: stringBuilder.ToString())
      );
    }

    private static string ScaleDownName(string input)
    {
      // good lord, help me with the maths homework

      // 30 is the max characters that can fit with font=100%
      // we need to scale it down accordingly and represent it as a percentage

      // remove all rich text tags from the string to get the actual length
      string name = Regex.Replace(input, @"<[^>]+>", string.Empty);

      // WEATHER:
      if (name.Length > 20)
      {
        int fontScaling = Mathf.Clamp((int)(100f * (20f / name.Length)), 65, 100);
        Plugin.debugLogger.LogDebug($"New font scale for string {name} is {fontScaling}%");

        return $"<size={fontScaling}%><cspace=-0.8px>{input}</size></cspace>";
      }
      else
      {
        return input;
      }
    }

    // it's like that because of weathertweaks
    private static string GetDisplayWeatherString(SelectableLevel level, Weather weather)
    {
      if (Settings.WeatherOverrideNames)
      {
        WeatherEffectOverride currentOverride = WeatherOverrideManager.GetCurrentWeatherOverride(level, weather);
        if (currentOverride != null && !string.IsNullOrEmpty(currentOverride.DisplayName))
        {
          return currentOverride.DisplayName;
        }
      }

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
