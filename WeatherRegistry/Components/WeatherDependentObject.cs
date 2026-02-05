// heavily inspired by Jacob's JWeatherObject https://github.com/JacobG55/JLL/blob/main/Components/JWeatherObject.cs

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace WeatherRegistry.Components
{
  public class WeatherDependentObject : MonoBehaviour
  {
    [Header("Weather Matching")]
    public string[] targetWeathers = [];
    internal LevelWeatherType[] resolvedWeathers = [];

    [Header("Targets")]
    public GameObject targetObject;

    [Tooltip("If true, the object will de-activate when the weather matches.")]
    public bool Inverse = false;

    [Header("Event Triggers")]
    public UnityEvent onActivate = new();
    public UnityEvent onDeactivate = new();

    public void Start()
    {
      if (targetObject == null)
      {
        targetObject = gameObject;
      }

      ToggleObjects();
    }

    public void ToggleObjects()
    {
      if (resolvedWeathers.Length == 0)
      {
        resolvedWeathers = targetWeathers
          .Select(w =>
          {
            return ConfigHelper.ResolveStringToWeather(w).VanillaWeatherType;
          })
          .ToArray();
      }

      if (resolvedWeathers.Length == 0)
      {
        Debug.LogWarning($"[WeatherRegistry] WeatherDependentObject on {gameObject.name} has no valid weathers to match against.");
        return;
      }

      bool isActiveWeather = resolvedWeathers.Contains(RoundManager.Instance.currentLevel.currentWeather);

      targetObject.SetActive(Inverse ? !isActiveWeather : isActiveWeather);
      (isActiveWeather ? onActivate : onDeactivate).Invoke();
    }
  }
}
