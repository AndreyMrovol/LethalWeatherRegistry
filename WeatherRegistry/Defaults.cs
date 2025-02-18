using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeatherRegistry
{
  internal class Defaults
  {
    internal static List<LevelWeatherType> VanillaWeathers = MrovLib.Defaults.VanillaWeathers;

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

    internal static Dictionary<LevelWeatherType, string> VanillaWeatherToWeatherWeights =
      new()
      {
        { LevelWeatherType.None, "None@160; Rainy@100; Stormy@70; Flooded@20; Foggy@40; Eclipsed@10" },
        { LevelWeatherType.Rainy, "None@100; Rainy@60; Stormy@40; Flooded@30; Foggy@50; Eclipsed@20" },
        { LevelWeatherType.Stormy, "None@160; Rainy@110; Stormy@10; Flooded@120; Foggy@20; Eclipsed@80" },
        { LevelWeatherType.Flooded, "None@160; Rainy@60; Stormy@50; Flooded@10; Foggy@60; Eclipsed@40" },
        { LevelWeatherType.Foggy, "None@200; Rainy@60; Stormy@50; Flooded@10; Foggy@30; Eclipsed@20" },
        { LevelWeatherType.Eclipsed, "None@300; Rainy@40; Stormy@16; Flooded@20; Foggy@60; Eclipsed@10" }
      };

    public static readonly string DefaultLevelFilters = "Company";
    public static readonly string DefaultLevelWeights = "MoonName@50";
    public static readonly string DefaultWeatherToWeatherWeights = "WeatherName@50";
    public static readonly int DefaultWeight = 100;
    public static readonly float ScrapAmountMultiplier = 1;
    public static readonly float ScrapValueMultiplier = 1;
    public static readonly FilteringOption FilteringOption = FilteringOption.Exclude;

    internal static Color LethalLibColor = new(r: 0f, g: 0.44f, b: 0.76f, a: 1f);

    internal static readonly string WeatherSaveKey = "WeatherRegistryCurrentWeathers";
  }
}
