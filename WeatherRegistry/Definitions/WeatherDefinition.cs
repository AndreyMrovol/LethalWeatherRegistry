using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Modules;
using WeatherRegistry.Patches;

namespace WeatherRegistry.Definitions
{
  public interface IWeatherDefinition
  {
    string Name { get; }
    WeatherEffectDefinition Effect { get; }
    AnimationClip AnimationClip { get; }
    Color Color { get; }
    RegistryWeatherConfig Configuration { get; }
  }

  public class WeatherDefinition : IWeatherDefinition
  {
    public string Name { get; set; } = "";
    public WeatherEffectDefinition Effect { get; set; } = new(null, null);
    public AnimationClip AnimationClip { get; set; } = null;
    public Color Color { get; set; }
    public RegistryWeatherConfig Configuration { get; set; } = new();

    // public Weather GetWeatherFromDefinition(){
    //   return new Weather(this);
    // }
  }
}
