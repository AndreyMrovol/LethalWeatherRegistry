using TMPro;
using UnityEngine;

namespace WeatherRegistry.Utils
{
  public static class ColorConverter
  {
    public static TMP_ColorGradient ToTMPColorGradient(Color color)
    {
      color.a = 1f;

      TMP_ColorGradient gradient = CreateColorGradientInstance();
      gradient.colorMode = ColorMode.Single;

      gradient.topLeft = color;
      gradient.topRight = color;
      gradient.bottomLeft = color;
      gradient.bottomRight = color;

      return gradient;
    }

    public static TMP_ColorGradient CreateColorGradientInstance()
    {
      return ScriptableObject.CreateInstance<TMP_ColorGradient>();
    }

    public static string ToHex(Color color)
    {
      return ColorUtility.ToHtmlStringRGB(color);
    }
  }
}
