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
  public class NewerWeatherDefinition : Definitions.WeatherDefinition
  {
    [Header("Configuration")]
    [SerializeField]
    [Tooltip("Your weather's default config values.")]
    public SerializableWeatherConfig EditorConfig;

    public void Reset()
    {
      Name = string.Empty;
      Effect = null;
      Color = ColorHelper.ToTMPColorGradient(UnityEngine.Color.cyan);

      Config = new();
    }
  }
}
