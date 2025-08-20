using UnityEngine;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "AdditionalWeatherEffect", menuName = "WeatherRegistry/Overrides/AdditionalWeatherEffect", order = 4)]
  public class AdditionalWeatherEffect : ScriptableObject
  {
    [Header("Matching properties")]
    [Tooltip("Weather names to match the override to")]
    public string[] weatherName;

    [Tooltip("Semicolon-separated list of level names to match the override to")]
    public string[] levelName;

    [Header("Effect properties")]
    public ImprovedWeatherEffect AdditionalEffect;
  }
}
