using Newtonsoft.Json;
using UnityEngine;

namespace WeatherRegistry.Editor
{
  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather Definition", menuName = "WeatherRegistry/WeatherDefinition", order = 100)]
  public class WeatherDefinition : ScriptableObject
  {
    [Header("Basic properties")]
    [SerializeField]
    [Tooltip("Your weather's name.")]
    public string Name;

    [SerializeField]
    [Tooltip("Your weather's color, displayed on the landing screen.")]
    public Color Color;

    [Header("Effect")]
    [Space(20)]
    [SerializeField]
    [Tooltip("Your weather's ImprovedWeatherEffect")]
    public ImprovedWeatherEffect Effect;

    [Header("Configuration")]
    [SerializeField]
    [Tooltip("Your weather's default config values.")]
    public SerializableWeatherConfig Config;

    public void Reset()
    {
      Name = string.Empty;
      Effect = null;
      Color = Color.cyan;

      Config = new();
    }
  }
}
