using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Enums;
using WeatherRegistry.Helpers;

namespace WeatherRegistry.Editor
{
  [JsonObject(MemberSerialization.OptIn)]
  [CreateAssetMenu(fileName = "Weather Definition", menuName = "WeatherRegistry/WeatherDefinition", order = 100)]
  public class NewerWeatherDefinition : WeatherRegistry.WeatherDefinition
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
      Color = ColorHelper.ToTMPColorGradient(UnityEngine.Color.cyan);

      Config = new();
    }
  }
}
