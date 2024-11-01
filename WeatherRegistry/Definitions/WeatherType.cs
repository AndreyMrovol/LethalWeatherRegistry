// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using BepInEx.Configuration;
// using Newtonsoft.Json;
// using UnityEngine;
// using WeatherRegistry.Definitions;
// using WeatherRegistry.Modules;
// using WeatherRegistry.Patches;

// namespace WeatherRegistry
// {
//   // [JsonObject(MemberSerialization.OptIn)]
//   // [CreateAssetMenu(fileName = "Weather", menuName = "WeatherRegistry/WeatherDefinition", order = 5)]
//   [Obsolete]
//   public class Weather : RegistryWeather
//   {
//     public Weather(string name = "None", ImprovedWeatherEffect effect = default)
//       : base(name, effect) { }

//     internal Weather(WeatherDefinition weatherDefinition)
//       : base(weatherDefinition) { }
//   }
// }
