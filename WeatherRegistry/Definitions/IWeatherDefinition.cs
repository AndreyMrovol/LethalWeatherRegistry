using TMPro;
using WeatherRegistry.Enums;

namespace WeatherRegistry.Definitions
{
  public interface IWeatherDefinition
  {
    public string Name { get; }
    public TMP_ColorGradient Color { get; }

    public ImprovedWeatherEffect Effect { get; }

    internal WeatherOrigin Origin { get; }
    internal WeatherType Type { get; }
    // internal LevelWeatherType VanillaWeatherType { get; }
  }

  // public class CWeatherDefinition
  // {
  //   public virtual string Name;
  //   public virtual TMP_ColorGradient Color;
  // }
}
