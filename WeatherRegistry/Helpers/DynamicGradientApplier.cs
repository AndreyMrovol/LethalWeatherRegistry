using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace WeatherRegistry.Helpers
{
  public class DynamicGradientApplier(TextMeshProUGUI textComponent)
  {
    public TextMeshProUGUI textComponent = textComponent;

    private class GradientInfo
    {
      public string content;
      public int originalPosition;
      public TMP_ColorGradient gradient;
      public int tagLength;
    }

    public void ApplyGradientsFromTags(string text)
    {
      // Pattern to match <gradient=name>content</gradient>
      string pattern = @"<gradient=""?([^"">]+)""?>(.*?)</gradient>";
      MatchCollection matches = Regex.Matches(text, pattern);

      if (matches.Count == 0)
      {
        textComponent.text = text;
        return;
      }

      // Store gradient info before removing tags
      List<GradientInfo> gradientInfos = [];

      foreach (Match match in matches)
      {
        string gradientName = match.Groups[1].Value;
        string content = match.Groups[2].Value;
        int originalPosition = match.Index;

        TMP_ColorGradient gradient = GetColoredString(gradientName);

        if (gradient != null)
        {
          gradientInfos.Add(
            new GradientInfo
            {
              content = content,
              originalPosition = originalPosition,
              gradient = gradient,
              tagLength = match.Length
            }
          );
        }
      }

      // Remove all gradient tags from text
      string cleanText = Regex.Replace(text, pattern, "$2");
      textComponent.text = cleanText;
      textComponent.ForceMeshUpdate();

      // Calculate actual positions and apply gradients
      int cumulativeOffset = 0;

      foreach (GradientInfo info in gradientInfos)
      {
        int actualStart = info.originalPosition - cumulativeOffset;
        int actualEnd = actualStart + info.content.Length - 1;

        ApplyGradient(actualStart, actualEnd, info.gradient);

        int tagOverhead = info.tagLength - info.content.Length;
        cumulativeOffset += tagOverhead;
      }
    }

    // Get gradients from settings
    TMP_ColorGradient GetColoredString(string text)
    {
      TMP_ColorGradient pickedColor = Settings.ScreenMapColors.TryGetValue(text, out TMP_ColorGradient value) ? value : new TMP_ColorGradient();

      if (pickedColor != new TMP_ColorGradient())
      {
        // add 10% of green value to make the editor color correctly display in-game (v70 screen color change)
        Color adjustedTopLeft = new(pickedColor.topLeft.r, pickedColor.topLeft.g * 1.1f, pickedColor.topLeft.b, pickedColor.topLeft.a);
        Color adjustedTopRight = new(pickedColor.topRight.r, pickedColor.topRight.g * 1.1f, pickedColor.topRight.b, pickedColor.topRight.a);
        Color adjustedBottomLeft =
          new(pickedColor.bottomLeft.r, pickedColor.bottomLeft.g * 1.1f, pickedColor.bottomLeft.b, pickedColor.bottomLeft.a);
        Color adjustedBottomRight =
          new(pickedColor.bottomRight.r, pickedColor.bottomRight.g * 1.1f, pickedColor.bottomRight.b, pickedColor.bottomRight.a);

        // Create a new gradient with adjusted colors (if you need to store it)
        pickedColor = new()
        {
          topLeft = adjustedTopLeft,
          topRight = adjustedTopRight,
          bottomLeft = adjustedBottomLeft,
          bottomRight = adjustedBottomRight,
          colorMode = pickedColor.colorMode,
          name = pickedColor.name
        };
      }

      Plugin.debugLogger.LogDebug($"Picked gradient for '{text}': {pickedColor.name}");

      return pickedColor;
    }

    // Core gradient application method
    //
    void ApplyGradient(int startChar, int endChar, TMP_ColorGradient gradient)
    {
      TMP_TextInfo textInfo = textComponent.textInfo;

      Color upperLeft = gradient.topLeft;
      Color upperRight = gradient.topRight;
      Color lowerLeft = gradient.bottomLeft;
      Color lowerRight = gradient.bottomRight;

      if (gradient.colorMode == ColorMode.Single)
      {
        upperLeft = gradient.topLeft;
        upperRight = gradient.topLeft;
        lowerLeft = gradient.topLeft;
        lowerRight = gradient.topLeft;
      }
      else if (gradient.colorMode == ColorMode.HorizontalGradient)
      {
        lowerLeft = upperLeft;
        lowerRight = upperRight;
      }
      else if (gradient.colorMode == ColorMode.VerticalGradient)
      {
        upperRight = upperLeft;
        lowerRight = lowerLeft;
      }

      for (int i = startChar; i <= endChar && i < textInfo.characterCount; i++)
      {
        if (!textInfo.characterInfo[i].isVisible)
        {
          continue;
        }

        int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
        int vertIndex = textInfo.characterInfo[i].vertexIndex;
        Color32[] colors = textInfo.meshInfo[matIndex].colors32;

        colors[vertIndex + 0] = lowerLeft;
        colors[vertIndex + 1] = upperLeft;
        colors[vertIndex + 2] = upperRight;
        colors[vertIndex + 3] = lowerRight;
      }

      textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
  }
}
