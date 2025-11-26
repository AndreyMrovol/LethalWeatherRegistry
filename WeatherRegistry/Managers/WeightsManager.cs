using System.Collections.Generic;
using System.Linq;
using MrovLib;
using WeatherRegistry.Enums;

namespace WeatherRegistry.Managers
{
  /// <summary>
  /// Manager responsible for handling weight-related functionalities.
  /// </summary>
  public static class WeightsManager
  {
    internal static List<LevelWeatherType> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      List<LevelWeatherType> possibleWeathers = level
        .randomWeathers.Where(randomWeather => randomWeather.weatherType != LevelWeatherType.None)
        .Select(x => x.weatherType)
        .Distinct()
        .ToList();

      // add None as a possible weather in front of the list
      possibleWeathers.Insert(0, LevelWeatherType.None);

      if (possibleWeathers == null || possibleWeathers.Count() == 0)
      {
        Plugin.logger.LogWarning("Level's random weathers are null");
        return [];
      }

      return possibleWeathers;
    }

    public static Utils.WeightHandler<Weather, WeatherWeightType> GetPlanetWeightedList(SelectableLevel level)
    {
      Utils.WeightHandler<Weather, WeatherWeightType> weightedList = new();
      Logger logger = WeatherCalculation.Logger;

      List<LevelWeatherType> weatherTypes = GetPlanetPossibleWeathers(level);

      if (weatherTypes == null || weatherTypes.Count() == 0)
      {
        Plugin.logger.LogWarning("Level's random weathers are null");
        return weightedList;
      }

      foreach (var weather in weatherTypes)
      {
        Weather typeOfWeather = WeatherManager.GetWeather(weather);

        (int weatherWeight, WeatherWeightType type) = typeOfWeather.GetWeightWithOrigin(level);
        weightedList.Add(typeOfWeather, weatherWeight);
      }

      return weightedList;
    }

    public static (int weight, WeatherWeightType type) GetWeatherWeightForLevel(SelectableLevel level, Weather weather)
    {
      Logger logger = WeatherCalculation.Logger;
      var weatherWeight = weather.DefaultWeight;
      WeatherWeightType weatherWeightType = WeatherWeightType.Default;

      var previousWeather = WeatherManager.GetWeather(level.currentWeather);

      if (previousWeather == null)
      {
        logger.LogError($"Previous weather is null for {level.name}");
      }

      // we have 3 weights possible:
      // 1. level weight
      // 2. weather-weather weights
      // 3. default weight
      // we want to execute them in this exact order

      if (weather.LevelWeights.TryGetValue(level, out int levelWeight))
      {
        // (1) => level weight
        // logger.LogDebug($"{this.Name} has level weight {levelWeight}");
        weatherWeightType = WeatherWeightType.Level;
        weatherWeight = levelWeight;
      }
      // try to get previous day weather (so - at this point - the current one)
      // but not on first day because that's unreliable and random (from my testing)
      else if (
        previousWeather.WeatherWeights.TryGetValue(weather.VanillaWeatherType, out int weatherWeightFromWeather)
        && StartOfRound.Instance.gameStats.daysSpent != 0
      )
      {
        // (2) => weather-weather weights

        // logger.LogDebug($"{this.Name} has weather>weather weight {weatherWeightFromWeather}");
        weatherWeightType = WeatherWeightType.WeatherToWeather;
        weatherWeight = weatherWeightFromWeather;
      }
      else
      {
        weatherWeightType = WeatherWeightType.Default;
        // logger.LogDebug($"{this.Name} has default weight {weatherWeight}");
      }

      return (weatherWeight, weatherWeightType);
    }
  }
}
