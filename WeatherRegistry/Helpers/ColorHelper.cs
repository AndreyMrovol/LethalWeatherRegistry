using TMPro;
using UnityEngine;

namespace WeatherRegistry.Helpers
{
  public static class ColorHelper
  {
    public static TMP_ColorGradient ToTMPColorGradient(Color color)
    {
      color.a = 1f;

      TMP_ColorGradient gradient = ScriptableObject.CreateInstance<TMP_ColorGradient>();
      gradient.colorMode = ColorMode.Single;

      gradient.topLeft = color;
      gradient.topRight = color;
      gradient.bottomLeft = color;
      gradient.bottomRight = color;

      return gradient;
    }

    public static string ToHex(Color color)
    {
      return ColorUtility.ToHtmlStringRGB(color);
    }
  }
}
