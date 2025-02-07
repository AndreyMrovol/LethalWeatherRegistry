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


    /// <summary>
    /// Sets the enable state of a weather effect for the current time of day.
    /// </summary>
    /// <param name="weatherType">The type of weather effect to modify. Must not be LevelWeatherType.None.</param>
    /// <param name="enabled">True to enable the effect, false to disable it.</param>
    /// <remarks>
    /// This method will not apply effects if:<br/>
    /// - The weather type is None<br/>
    /// - The player is currently inside a building<br/>
    /// - The specified weather effect doesn't exist in TimeOfDay.Instance.effects
    /// </remarks>
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

      DisableWeatherEffects();

      if (weather == null)
      {
        return;
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

      if (weather.VanillaWeatherType == LevelWeatherType.None)
      {
        SunAnimator.OverrideSunAnimator(weather.VanillaWeatherType);
        return;
      }

      // check if JLL does weather override
      if (Plugin.JLLCompat.IsModPresent)
      {
        (bool isDoingOverride, WeatherEffect JLLEffect) = Plugin.JLLCompat.GetJLLData();

        if (isDoingOverride)
        {
          Plugin.logger.LogInfo("Enabling JLL WeatherEffect");
          JLLEffect.effectEnabled = true;
          JLLEffect.effectObject?.SetActive(true);

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

    public static void DisableWeatherEffects()
    {
      // disable all weather effects
      foreach (WeatherEffectOverride effectOverride in WeatherManager.WeatherEffectOverrides)
      {
        effectOverride.OverrideEffect.DisableEffect(true);
      }

      foreach (ImprovedWeatherEffect effect in WeatherManager.Weathers.Select(weather => weather.Effect))
      {
        effect.DisableEffect(true);
      }

      if (Plugin.JLLCompat.IsModPresent)
      {
        (bool isDoingOverride, WeatherEffect JLLEffect) = Plugin.JLLCompat.GetJLLData();

        if (isDoingOverride)
        {
          Plugin.logger.LogInfo("Disabling JLL WeatherEffect");
          JLLEffect.effectEnabled = false;
          JLLEffect.effectObject?.SetActive(false);
        }
      }
    }

    public static void EnableCurrentWeatherEffects()
    {
      // check if JLL does weather override
      if (Plugin.JLLCompat.IsModPresent)
      {
        (bool isDoingOverride, WeatherEffect JLLEffect) = Plugin.JLLCompat.GetJLLData();

        if (isDoingOverride)
        {
          Plugin.logger.LogInfo("Enabling JLL WeatherEffect");
          JLLEffect.effectEnabled = true;
          JLLEffect.effectObject?.SetActive(true);
        }
      }

      foreach (LevelWeatherType weatherType in WeatherManager.CurrentEffectTypes)
      {
        Weather weather = WeatherManager.GetWeather(weatherType);
        weather.Effect.EffectEnabled = true;
        SetTimeOfDayEffect(weather.VanillaWeatherType, true);
      }
    }

    public static void HandleJLLOverride(LevelWeatherType weatherType, bool enable)
    {
      if (Plugin.JLLCompat.IsJLLDoingWeatherOverride())
      {
        // if JLL is doing weather override, it's gonna enable their effect on its own
        // so we just disable all effects and JLL does the rest
        Plugin.logger.LogInfo("Detected JLL WeatherOverride");
        return;
      }
    }
  }
}
