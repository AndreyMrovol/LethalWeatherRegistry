using TMPro;
using UnityEngine;

namespace WeatherRegistry.Helpers
{
  public static class ColorHelper
  {
    public static TMP_ColorGradient ToTMPColorGradient(Color color)
    {
      return new TMP_ColorGradient(color);
    }
  }
}
