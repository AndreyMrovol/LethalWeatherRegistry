using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Obsolete]
    [HideInInspector]
    public Color Color;

    [SerializeField]
    [Tooltip("Your weather's color, displayed on the landing screen.")]
    [FormerlySerializedAs("Color")]
    public TMP_ColorGradient ColorGradient;

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

      Color = UnityEngine.Color.cyan;

      Config = new();
    }
  }
}
