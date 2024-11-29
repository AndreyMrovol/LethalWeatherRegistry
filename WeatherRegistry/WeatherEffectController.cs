using System;
using System.Collections.Generic;
using System.Linq;
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;
using WeatherRegistry.Patches;

namespace WeatherRegistry
{
  public static class WeatherEffectController
  {
    // TODO: this can be called while in orbit lol
    // TODO: allow for calling this from WeatherController

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

      if (weather == null || weather.VanillaWeatherType == LevelWeatherType.None)
      {
        return;
      }

      // enable current weather effect
      WeatherEffectOverride weatherEffectOverride = weather.GetEffectOverride(currentLevel);
      if (weatherEffectOverride == null)
      {
        weather.Effect.EffectEnabled = true;
        TimeOfDay.Instance.effects[(int)weather.VanillaWeatherType].effectEnabled = true;
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
        TimeOfDay.Instance.effects[(int)weather.VanillaWeatherType].effectEnabled = true;
      }
    }
  }
}
