using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WeatherRegistry.Definitions
{
  public class CurrentWeathers
  {
    private Dictionary<SelectableLevel, LevelWeatherType> _currentWeathers = [];

    public Dictionary<SelectableLevel, LevelWeatherType> Entries
    {
      get { return _currentWeathers; }
      internal set { _currentWeathers = value; }
    }

    public void Initialize()
    {
      _currentWeathers = [];
    }

    public void Refresh()
    {
      Plugin.debugLogger.LogDebug("Refreshing current weathers");
      List<SelectableLevel> levels = GetLevels();
      Clear();

      foreach (SelectableLevel level in levels)
      {
        if (!_currentWeathers.ContainsKey(level))
        {
          _currentWeathers[level] = level.currentWeather;
        }
      }
    }

    public void Clear()
    {
      _currentWeathers.Clear();
    }

    public List<SelectableLevel> GetLevels()
    {
      return MrovLib.SharedMethods.GetGameLevels();
    }

    public bool Contains(LevelWeatherType weatherType)
    {
      return _currentWeathers.ContainsValue(weatherType);
    }

    public bool Contains(Weather weather)
    {
      return Contains(weather.VanillaWeatherType);
    }

    public bool Contains(SelectableLevel level)
    {
      return _currentWeathers.ContainsKey(level);
    }

    public Dictionary<string, LevelWeatherType> GetWeathers => _currentWeathers.ToDictionary(pair => pair.Key.PlanetName, pair => pair.Value);
    public string SerializedEntries => JsonConvert.SerializeObject(GetWeathers);

    public WeatherSyncData[] SyncData
    {
      get { return _currentWeathers.Select(pair => new WeatherSyncData() { LevelName = pair.Key.PlanetName, Weather = pair.Value }).ToArray(); }
    }

    public void CallSync()
    {
      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      WeatherSync.Instance.SetNewOnHost(GetWeathers);
    }

    public LevelWeatherType GetWeatherType(SelectableLevel level)
    {
      return _currentWeathers[level];
    }

    public Weather GetLevelWeather(SelectableLevel level)
    {
      return WeatherManager.GetWeather(_currentWeathers[level]);
    }

    // this is the one that every overload should resolve to
    public void SetWeather(SelectableLevel level, LevelWeatherType weatherType)
    {
      _currentWeathers[level] = weatherType;
      CallSync();
    }

    public void SetWeather(SelectableLevel level, Weather weather)
    {
      SetWeather(level, weather.VanillaWeatherType);
    }

    public void OverrideWeathers(Dictionary<SelectableLevel, LevelWeatherType> weathers)
    {
      _currentWeathers = weathers;
      CallSync();
    }

    public void OverrideWeathers(List<(SelectableLevel, LevelWeatherType)> weathers)
    {
      OverrideWeathers(weathers.ToDictionary(pair => pair.Item1, pair => pair.Item2));
    }

    public void OverrideWeathers(string serializedWeathers)
    {
      OverrideWeathers(JsonConvert.DeserializeObject<Dictionary<SelectableLevel, LevelWeatherType>>(serializedWeathers));
    }

    public void SetWeathers(Dictionary<SelectableLevel, LevelWeatherType> weathers)
    {
      foreach (KeyValuePair<SelectableLevel, LevelWeatherType> pair in weathers)
      {
        _currentWeathers[pair.Key] = pair.Value;
      }

      CallSync();
    }

    public void SetWeathers(List<(SelectableLevel, LevelWeatherType)> weathers)
    {
      SetWeathers(weathers.ToDictionary(pair => pair.Item1, pair => pair.Item2));
    }

    public void SetWeathers(string serializedWeathers)
    {
      SetWeathers(JsonConvert.DeserializeObject<Dictionary<SelectableLevel, LevelWeatherType>>(serializedWeathers));
    }

    public void SetWeathersFromStringDictionary(string serializedWeathers)
    {
      Plugin.logger.LogDebug($"Setting weathers from string dictionary: {serializedWeathers}");

      Dictionary<string, LevelWeatherType> planetNameDictionary = JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(
        serializedWeathers
      );
      Dictionary<SelectableLevel, LevelWeatherType> weathers = [];
      List<SelectableLevel> levels = GetLevels();

      // check if any levels are missing from the level list (moon was removed between launches)
      foreach (KeyValuePair<string, LevelWeatherType> pair in planetNameDictionary)
      {
        SelectableLevel level = levels.Find(l => l.PlanetName == pair.Key);

        if (level == null)
        {
          Plugin.logger.LogWarning($"Level with planet name {pair.Key} is not present, skipping.");
          continue;
        }

        // check if any weathers were removed between launches
        if (WeatherManager.GetWeather(pair.Value) == null)
        {
          Plugin.logger.LogWarning($"Weather with type {pair.Value} was not found - setting to None.");
          weathers[level] = LevelWeatherType.None;
          continue;
        }

        weathers[level] = pair.Value;
      }

      // check if any levels are missing from the dictionary (moon was added between launches)
      foreach (SelectableLevel level in levels)
      {
        if (!weathers.ContainsKey(level))
        {
          Plugin.logger.LogWarning($"Level with planet name {level.PlanetName} was not found - setting to None.");
          weathers[level] = LevelWeatherType.None;
        }
      }

      SetWeathers(weathers);
    }
  }
}
