using UnityEngine;

namespace WeatherRegistry.Editor
{
  [CreateAssetMenu(fileName = "PlanetNameOverride", menuName = "WeatherRegistry/Overrides/PlanetNameOverride", order = 5)]
  public class PlanetNameOverride : ScriptableObject
  {
    [Header("Matching properties")]
    [Tooltip("ImprovedWeatherEffect to match")]
    public ImprovedWeatherEffect effectOverride;

    [Header("Display properties")]
    [Tooltip("The planet name when the override is applied")]
    public string newPlanetName;
  }
}
