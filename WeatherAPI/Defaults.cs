using System.Collections.Generic;
using UnityEngine;

namespace WeatherAPI
{
  internal class Defaults
  {
    internal static List<LevelWeatherType> VanillaWeathers =
    [
      LevelWeatherType.None,
      LevelWeatherType.DustClouds,
      LevelWeatherType.Foggy,
      LevelWeatherType.Rainy,
      LevelWeatherType.Stormy,
      LevelWeatherType.Flooded,
      LevelWeatherType.Eclipsed
    ];

    internal static Dictionary<LevelWeatherType, Color> VanillaWeatherColors =
      new()
      {
        { LevelWeatherType.None, new Color(0.41f, 1f, 0.42f, 1f) },
        { LevelWeatherType.DustClouds, new Color(0.41f, 1f, 0.42f, 1f) },
        { LevelWeatherType.Foggy, new Color(1f, 0.86f, 0f, 1f) },
        { LevelWeatherType.Rainy, new Color(1f, 0.86f, 0f, 1f) },
        { LevelWeatherType.Stormy, new Color(1f, 0.57f, 0f, 1f) },
        { LevelWeatherType.Flooded, new Color(1f, 0.57f, 0f, 1f) },
        { LevelWeatherType.Eclipsed, new Color(1f, 0f, 0f, 1f) }
      };

    internal static Color LethalLibColor = new(r: 0f, g: 0.44f, b: 0.76f, a: 1f);
  }
}
