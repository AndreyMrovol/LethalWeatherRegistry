using System;
using UnityEngine;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "ModdedWeathersMatcher", menuName = "WeatherRegistry/ModdedWeathersMatcher", order = 200)]
  public class ModdedWeathersMatcher : ScriptableObject
  {
    [field: SerializeField]
    public SelectableLevel Level { get; set; }

    [field: SerializeField]
    public WeatherMatchingProperties[] Weathers { get; set; } = [];

    public void Reset()
    {
      Weathers = [];
    }
  }

  [Serializable]
  public class WeatherMatchingProperties
  {
    [field: Header("Matching Properties")]
    [field: SerializeField]
    [field: Tooltip("Name of the weather to match with (case-insensitive, whitespace-insensitive)")]
    public string Name { get; set; }

    [field: Space(5)]
    [field: Header("Weather Properties")]
    [field: SerializeField]
    [field: Range(0, 1000)]
    public int DefaultLevelWeight { get; set; }

    public void Reset()
    {
      Name = "";
      DefaultLevelWeight = 100;
    }
  }
}
