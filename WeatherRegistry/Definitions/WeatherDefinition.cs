using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WeatherRegistry.Enums;
using WeatherRegistry.Helpers;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Definitions
{
  //TODO: is that really needed for anything?

  [JsonObject(MemberSerialization.OptIn)]
  public class WeatherDefinition : ScriptableObject
  {
    public string Name { get; set; }

    public TMP_ColorGradient Color { get; set; } = ColorHelper.ToTMPColorGradient(UnityEngine.Color.cyan);

    public ImprovedWeatherEffect Effect { get; set; }

    public bool Enabled { get; set; }

    public RegistryWeatherConfig Config { get; set; }

    public WeatherOrigin Origin { get; private set; } = WeatherOrigin.WeatherRegistry;
    public WeatherType Type { get; private set; } = WeatherType.Modded;

    public override string ToString()
    {
      return $"<color=#{ColorHelper.ToHex(Color.topLeft)}>{Name} ({Origin}, {Type})</color>";
    }
  }
}
