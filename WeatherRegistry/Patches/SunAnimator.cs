using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace WeatherRegistry.Patches
{
  internal class SunAnimator
  {
    public static void Init()
    {
      // create separate harmony instance for Animator
      var harmony = new Harmony("WeatherRegistry.SunAnimator");

      // patch the Animator class with the SetBool method
      harmony.Patch(
        AccessTools.Method(typeof(UnityEngine.Animator), "SetBool", new Type[] { typeof(string), typeof(bool) }),
        new HarmonyMethod(typeof(SunAnimator), "SetBoolStringPatch")
      );
      logger.LogDebug("Patching Animator.SetBool(string, bool)");
    }

    public static bool SetBoolPatch(Animator __instance, object nameOrId, bool value)
    {
      string name = nameOrId as string;

      if (TimeOfDay.Instance == null)
      {
        return true;
      }

      if (name == "overcast" || name == "eclipse")
      {
        if (ConfigManager.SunAnimatorBlacklistLevels.Contains(StartOfRound.Instance.currentLevel))
        {
          return true;
        }
      }

      return true;
    }

    public static bool SetBoolStringPatch(Animator __instance, string name, bool value)
    {
      return SetBoolPatch(__instance, name, value);
    }

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherRegistry SunAnimator");

    internal static Dictionary<string, LevelWeatherType> vanillaBools =
      new()
      {
        { "", LevelWeatherType.None },
        { "overcast", LevelWeatherType.Stormy },
        { "eclipse", LevelWeatherType.Eclipsed },
      };

    internal static Dictionary<LevelWeatherType, string> clipNames =
      new()
      {
        { LevelWeatherType.None, "" },
        { LevelWeatherType.Stormy, "Stormy" },
        { LevelWeatherType.Eclipsed, "Eclipse" },
      };

    internal static List<string> animatorControllerBlacklist = ["SunAnimContainerCompanyLevel"];

    public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
      public AnimationClipOverrides(int capacity)
        : base(capacity) { }

      public AnimationClip this[string name]
      {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
          int index = this.FindIndex(x => x.Key.name.Equals(name));
          if (index != -1)
            this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
      }
    }

    internal static AnimatorOverrideController animatorOverrideController;

    public static void OverrideSunAnimator(LevelWeatherType weatherType)
    {
      Plugin.logger.LogDebug("OverrideSunAnimator called");

      if (ConfigManager.SunAnimatorBlacklistLevels.Contains(StartOfRound.Instance.currentLevel))
      {
        logger.LogDebug($"Current level {StartOfRound.Instance.currentLevel} is blacklisted");
        return;
      }

      if (TimeOfDay.Instance.sunAnimator == null)
      {
        logger.LogDebug("sunAnimator is null, skipping");
        return;
      }

      AnimatorClipInfo[] currentClips = TimeOfDay.Instance.sunAnimator.GetCurrentAnimatorClipInfo(0);
      if (currentClips.Length == 0)
      {
        logger.LogDebug("There are no SunAnimator clips, skipping");
        return;
      }

      logger.LogInfo($"Current clip: {currentClips[0].clip.name}");

      // get the name of the sun TimeOfDay.Instance.sunAnimator controller
      string animatorControllerName = TimeOfDay.Instance.sunAnimator.runtimeAnimatorController.name;
      logger.LogInfo($"animatorControllerName: {animatorControllerName}, weatherType: {weatherType}");

      if (animatorControllerBlacklist.Contains(animatorControllerName))
      {
        logger.LogDebug($"TimeOfDay.Instance.sunAnimator controller {animatorControllerName} is blacklisted");
        return;
      }

      if (animatorOverrideController == null)
      {
        animatorOverrideController = new AnimatorOverrideController(TimeOfDay.Instance.sunAnimator.runtimeAnimatorController)
        {
          name = $"{animatorControllerName}override"
        };
      }

      AnimationClipOverrides clipOverrides = new(animatorOverrideController.overridesCount);
      logger.LogDebug($"Overrides: {animatorOverrideController.overridesCount}");
      animatorOverrideController.GetOverrides(clipOverrides);

      var animationClips = animatorOverrideController.runtimeAnimatorController.animationClips.ToList();

      Dictionary<LevelWeatherType, AnimationClip> clips = [];

      Weather currentWeather = WeatherManager.GetWeather(weatherType);

      try
      {
        AnimationClip clipEclipsed = animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Eclipsed]));
        AnimationClip clipStormy = animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Stormy]));
        AnimationClip clipNone = animationClips.Find(clip =>
          !clip.name.Contains(clipNames[LevelWeatherType.Stormy]) && !clip.name.Contains(clipNames[LevelWeatherType.Eclipsed])
        );

        clips = new Dictionary<LevelWeatherType, AnimationClip>()
        {
          { LevelWeatherType.Eclipsed, clipEclipsed },
          { LevelWeatherType.Stormy, clipStormy },
          { LevelWeatherType.Flooded, clipStormy },
          { LevelWeatherType.Foggy, clipStormy },
          { LevelWeatherType.Rainy, clipStormy },
          { LevelWeatherType.None, clipNone },
        };

        // TODO: this requires testing + correct implementation (to be extensible by weathertweaks)

        if (WeatherManager.GetWeatherAnimationClip(weatherType) != null)
        {
          AnimationClip customAnimationClip = WeatherManager.GetWeatherAnimationClip(weatherType);

          TimeOfDay.Instance.sunAnimator.runtimeAnimatorController.animationClips.Add(customAnimationClip);
          animationClips.Add(customAnimationClip);
          clips[weatherType] = customAnimationClip;

          logger.LogInfo($"Added animation clip for weather type {weatherType}");

          // log all clips
          TimeOfDay
            .Instance.sunAnimator.runtimeAnimatorController.animationClips.ToList()
            .ForEach(clip =>
            {
              logger.LogInfo($"clip: {clip.name}");
            });
        }
        else if (currentWeather.Type != WeatherType.Vanilla)
        {
          logger.LogDebug($"No custom animation clip found for weather type {weatherType}");
          logger.LogDebug("Trying to apply vanilla animator bool");

          bool doesUseVanillaBool = vanillaBools.TryGetValue(currentWeather.Effect.SunAnimatorBool, out LevelWeatherType vanillaBool);

          if (!doesUseVanillaBool)
          {
            logger.LogInfo($"No vanilla bool found for weather type {weatherType}");
            return;
          }

          clips[weatherType] = clips[vanillaBools[currentWeather.Effect.SunAnimatorBool]];
        }

        if (clipEclipsed == null || clipStormy == null || clipNone == null)
        {
          return;
        }
      }
      catch (Exception e)
      {
        logger.LogWarning($"Detected a null clip: {e.Message}");
        return;
      }

      logger.LogDebug($"Clips: {clips.Count}");

      if (clips.Keys.Select(key => key == weatherType).Count() == 0)
      {
        logger.LogDebug($"No animation clip found for weather type {weatherType}");
        return;
      }

      // try to get clip names dynamically from the animator controller
      // use contains to check through the dictionary

      // get the name of the animation clip for the current weather type
      string animationClipName = clips.TryGetValue(weatherType, out AnimationClip clip) ? clip.name : null;
      if (animationClipName == null)
      {
        logger.LogDebug($"No animation clip found for weather type {weatherType}");
        return;
      }

      clips
        .ToList()
        .ForEach(clipPair =>
        {
          // override only clips different than the new one

          if (clipPair.Key != weatherType)
          {
            clipOverrides[clipPair.Value.name] = clips[weatherType];
            logger.LogDebug($"Setting override from {clipPair.Value.name} to {clips[weatherType].name}");
          }
          else
          {
            clipOverrides[clipPair.Value.name] = null;
            logger.LogDebug($"Setting override from {clipPair.Value.name} to null");
          }
        });

      logger.LogDebug(
        $"Current bools: {TimeOfDay.Instance.sunAnimator.GetBool("overcast")} {TimeOfDay.Instance.sunAnimator.GetBool("eclipsed")}"
      );

      if (weatherType != LevelWeatherType.None)
      {
        animatorOverrideController.ApplyOverrides(clipOverrides);
        TimeOfDay.Instance.sunAnimator.runtimeAnimatorController = animatorOverrideController;
      }
      else
      {
        TimeOfDay.Instance.sunAnimator.runtimeAnimatorController = animatorOverrideController.runtimeAnimatorController;
      }

      logger.LogInfo($"Current clip: {TimeOfDay.Instance.sunAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");
    }

    internal static void LogOverrides(AnimationClipOverrides clipOverrides)
    {
      logger.LogDebug($"Overrides: {clipOverrides.Count}");
      clipOverrides
        .ToList()
        .ForEach(clip =>
        {
          logger.LogInfo($"overrideclip {(clip.Key ? clip.Key.name : "null")} : {(clip.Value ? clip.Value.name : "null")}");
        });
    }

    internal static void Clear()
    {
      // animator = null;
      animatorOverrideController = null;
    }
  }
}
