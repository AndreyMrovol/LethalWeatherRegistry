using System;
using System.Collections.Generic;
using System.Linq;
using JLL.Components;
using UnityEngine;
using WeatherRegistry.Compatibility;
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  public static class WeatherEffectController
  {
    // TODO: this can be called while in orbit lol
    // TODO: allow for calling this from WeatherController

    internal static void SetTimeOfDayEffect(LevelWeatherType weatherType, bool enabled)
    {
      if (weatherType == LevelWeatherType.None)
      {
        return;
      }

      if (EntranceTeleportPatch.isPlayerInside)
      {
        Plugin.logger.LogWarning("Player is inside, not setting time of day effect");
        return;
      }

      if (TimeOfDay.Instance.effects[(int)weatherType] != null)
      {
        Plugin.debugLogger.LogDebug($"Setting time of day effect {weatherType} to {enabled}");
        TimeOfDay.Instance.effects[(int)weatherType].effectEnabled = enabled;
      }
    }

    // this is the overload that everything should resolve to
    public static void SetWeatherEffects(Weather weather)
    {
      SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;

      // disable all weather effects
      foreach (WeatherEffectOverride effectOverride in WeatherManager.WeatherEffectOverrides)
      {
        effectOverride.OverrideEffect.DisableEffect(true);
      }

      foreach (ImprovedWeatherEffect effect in WeatherManager.Weathers.Select(weather => weather.Effect))
      {
        effect.DisableEffect(true);
      }

      // if weather is not flooded, stop player from sinking
      if (weather.VanillaWeatherType != LevelWeatherType.Flooded)
      {
        var player = GameNetworkManager.Instance.localPlayerController;
        player.isUnderwater = false;
        player.sourcesCausingSinking = Mathf.Clamp(player.sourcesCausingSinking - 1, 0, 100);
        player.isMovementHindered = Mathf.Clamp(player.isMovementHindered - 1, 0, 100);
        player.hinderedMultiplier = 1f;
      }

      if (weather == null)
      {
        return;
      }

      if (weather.VanillaWeatherType == LevelWeatherType.None)
      {
        try
        {
          SunAnimator.OverrideSunAnimator(weather.VanillaWeatherType);
        }
        catch (Exception e)
        {
          Plugin.logger.LogError($"SunAnimator exception: {e.Message}");
          Plugin.logger.LogWarning("PLEASE report this issue to the mod developer with your modpack code and this log!");
        }

        return;
      }

      // check if JLL does weather override
      if (Plugin.JLLCompat.IsModPresent)
      {
        if (Plugin.JLLCompat.IsJLLDoingWeatherOverride())
        {
          Plugin.logger.LogInfo("Detected JLL WeatherOverride");
          return;
        }
      }

      // enable current weather effect
      WeatherEffectOverride weatherEffectOverride = weather.GetEffectOverride(currentLevel);
      if (weatherEffectOverride == null)
      {
        weather.Effect.EffectEnabled = true;
        SetTimeOfDayEffect(weather.VanillaWeatherType, true);
      }
      else
      {
        weather.Effect.EffectEnabled = false;
        weatherEffectOverride.OverrideEffect.EffectEnabled = true;
      }

      try
      {
        SunAnimator.OverrideSunAnimator(weather.VanillaWeatherType);
      }
      catch (Exception e)
      {
        Plugin.logger.LogError($"SunAnimator exception: {e.Message}");
        Plugin.logger.LogWarning("PLEASE report this issue to the mod developer with your modpack code and this log!");
      }
    }

    public static void SetWeatherEffects(LevelWeatherType weatherType)
    {
      Weather weather = WeatherManager.GetWeather(weatherType);
      SetWeatherEffects(weather);
    }

    public static void EnableCurrentWeatherEffects()
    {
      foreach (LevelWeatherType weatherType in WeatherManager.CurrentEffectTypes)
      {
        Weather weather = WeatherManager.GetWeather(weatherType);
        weather.Effect.EffectEnabled = true;
        SetTimeOfDayEffect(weather.VanillaWeatherType, true);
      }
    }
  }
}
