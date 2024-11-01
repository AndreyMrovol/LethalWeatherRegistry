using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Patches;

namespace WeatherRegistry.Definitions
{
  public interface IWeatherDefinition
  {
    string Name { get; }
    ImprovedWeatherEffect Effect { get; }
    AnimationClip AnimationClip { get; }
    Color Color { get; }
    IWeatherConfigDefinition Configuration { get; }
  }

  public class WeatherDefinition : IWeatherDefinition
  {
    public string Name { get; set; }
    public ImprovedWeatherEffect Effect { get; set; }
    public AnimationClip AnimationClip { get; set; }
    public Color Color { get; set; }
    public IWeatherConfigDefinition Configuration { get; set; }

    public WeatherDefinition()
    {
      Configuration = new WeatherConfigDefinition();
    }
  }
}
