using System;
using UnityEngine;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "ImprovedRandomWeathers", menuName = "WeatherRegistry/ImprovedRandomWeathers", order = 200)]
  public class ImprovedRandomWeathers : ScriptableObject
  {
    [SerializeField]
    public LevelWeatherMatcher[] LevelWeathers;
  }

  [Serializable]
  public class LevelWeatherMatcher
  {
    [SerializeField]
    [Tooltip("The name of the SelectableLevel (PlanetName) to match - case-insensitive, whitespace-insensitive")]
    public string LevelName;

    [SerializeField]
    [Header("Random Weather Settings")]
    [Tooltip("Weathers to include/exclude for this level - setting this will override SelectableLevel.randomWeathers !!")]
    public RandomWeatherEntry[] Weathers;

    public void Reset()
    {
      Weathers = [];
    }

    public override string ToString()
    {
      return $"{LevelName} (Weathers: {Weathers.Length})";
    }
  }

  [Serializable]
  public class RandomWeatherEntry
  {
    [Header("Weather Matching")]
    [Tooltip("Name of the weather to include/exclude (case-insensitive, whitespace-insensitive)")]
    public string WeatherName;

    [Header("Weather Variables (Vanilla)")]
    public int Variable;
    public int Variable2;

    [Header("Registry Options")]
    [Tooltip("Should this weather be excluded from being allowed on the moon?")]
    public bool Blacklist;

    public int DefaultLevelWeight = Defaults.DefaultWeight;
  }
}
