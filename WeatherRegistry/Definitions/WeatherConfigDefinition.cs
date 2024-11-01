using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace WeatherRegistry.Definitions
{
  public interface IWeatherConfigDefinition
  {
    ConfigEntryAbstraction<int> DefaultWeight { get; }
    ConfigEntryAbstraction<float> ScrapAmountMultiplier { get; }
    ConfigEntryAbstraction<float> ScrapValueMultiplier { get; }
  }

  public class WeatherConfigDefinition : IWeatherConfigDefinition
  {
    public ConfigEntryAbstraction<int> DefaultWeight { get; set; }
    public ConfigEntryAbstraction<float> ScrapAmountMultiplier { get; set; }
    public ConfigEntryAbstraction<float> ScrapValueMultiplier { get; set; }
  }
}
