using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MrovLib;
using TMPro;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Helpers;

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

      DynamicGradientApplier gradientApplier = new(___screenLevelDescription);

      Regex multiNewLine = new(@"\n{2,}");
      string planetName = ___currentLevel.PlanetName;

      Weather currentWeather = WeatherManager.GetCurrentWeather(___currentLevel);
      if (Managers.WeatherOverrideManager.GetCurrentWeatherOverride(___currentLevel, currentWeather) is WeatherEffectOverride currentOverride)
      {
        string newName = Managers.WeatherOverrideManager.GetPlanetOverrideName(currentOverride);

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

      gradientApplier.ApplyGradientsFromTags(___screenLevelDescription.text);

      __instance.screenLevelVideoReel.enabled = Settings.PlanetVideos;
      Plugin.debugLogger.LogDebug("Video reel enabled: " + Settings.PlanetVideos);

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
        WeatherEffectOverride currentOverride = Managers.WeatherOverrideManager.GetCurrentWeatherOverride(level, weather);
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

          // resolve weather name string into color using Settings.ScreenMapColors dictionary
          // so other mods (like weathertweaks) can add their own colors and symbols

          TMP_ColorGradient pickedColor = Settings.ScreenMapColors.TryGetValue(newWord, out TMP_ColorGradient value)
            ? value
            : new TMP_ColorGradient();

          outputString += pickedColor != new TMP_ColorGradient() ? $"<gradient={newWord}>{word}</gradient>" : $"{newWord}";
        });

      return outputString;
    }
  }
}
