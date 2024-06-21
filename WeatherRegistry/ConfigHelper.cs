using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace WeatherRegistry
{
  public class Rarity
  {
    public int Weight { get; set; }
  }

  public class NameRarity : Rarity
  {
    public string Name { get; set; }
  }

  public class LevelRarity : Rarity
  {
    public SelectableLevel Level { get; set; }
  }

  public class WeatherRarity : Rarity
  {
    public Weather Weather { get; set; }
  }

  public class ConfigHandler<T, CT>
  {
    // i need to make some stupid shit
    // basically a property that wraps around Bepinex ConfigEntry
    // so a getter would read that and resolve to my types
    // and a setter that does <something>

    // and this will resolve to string
    // so bepinex won't shit the bed hehe

    internal ConfigEntry<CT> _configEntry { get; set; }

    internal CT DefaultValue { get; set; }

    public virtual T Value { get; } = default(T);

    // forgive me for doing this
  }

  public class LevelListConfigHandler : ConfigHandler<SelectableLevel[], string>
  {
    public override SelectableLevel[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToLevels(_configEntry.Value);
      }
    }
  }

  public class LevelWeightsConfigHandler : ConfigHandler<LevelRarity[], string>
  {
    public override LevelRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToLevelRarities(_configEntry.Value);
      }
    }
  }

  public class WeatherWeightsConfigHandler : ConfigHandler<WeatherRarity[], string>
  {
    public override WeatherRarity[] Value
    {
      get
      {

        return ConfigHelper.ConvertStringToWeatherWeights(_configEntry.Value);
      }
    }
  }

  public class IntegerConfigHandler : ConfigHandler<int, int>
  {
    public override int Value
    {
      get { return _configEntry.Value; }
    }
  }

  internal class ConfigHelper
  {
    internal static Dictionary<string, SelectableLevel> _dictionary = null;
    public static Dictionary<string, SelectableLevel> StringToLevel
    {
      get
      {
        if (_dictionary != null)
        {
          return _dictionary;
        }

        Dictionary<string, SelectableLevel> Levels = [];

        StartOfRound
          .Instance.levels.ToList()
          .ForEach(level =>
          {
            Levels.TryAdd(GetNumberlessName(level).ToLower(), level);
            Levels.TryAdd(level.PlanetName.ToLower(), level);
            Levels.TryAdd(level.name.ToLower(), level);
          });

        _dictionary = Levels;

        // return the result of this setter
        return Levels;
      }
    }

    // straight-up copied from LLL (it's so fucking useful)
    public static string GetNumberlessName(SelectableLevel level)
    {
      return new string(level.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
    }

    // convert string to array of strings
    public static string[] ConvertStringToArray(string str)
    {
      string[] output = str.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray().Select(s => s.Trim()).ToArray();

      return output;
    }

    public static SelectableLevel[] ConvertStringToLevels(string str)
    {
      string[] levelNames = ConvertStringToArray(str);
      Dictionary<string, SelectableLevel> Levels = StringToLevel;
      // StartOfRound.Instance.levels.ToDictionary(level => GetNumberlessName(level), level => level);
      List<SelectableLevel> output = [];

      if (levelNames.Count() == 0)
      {
        return [];
      }

      foreach (string level in levelNames)
      {
        SelectableLevel selectableLevel = Levels.GetValueOrDefault(level);

        Plugin.logger.LogDebug($"Selectable level: {selectableLevel}");

        if (output.Contains(selectableLevel))
        {
          continue;
        }

        output.Add(selectableLevel);
      }

      return output.ToArray();
    }

    public static NameRarity[] ConvertStringToRarities(string str)
    {
      string[] rarities = ConvertStringToArray(str);
      List<NameRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split(':');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogError($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogError($"Invalid rarity weight: {rarityData[1]}");
          continue;
        }

        output.Add(new NameRarity { Name = rarityData[0], Weight = weight });
      }

      return output.ToArray();
    }

    public static LevelRarity[] ConvertStringToLevelRarities(string str)
    {
      // i want to use the following format:
      // LevelName@Weight;LevelName@Weight;LevelName@Weight

      string[] rarities = ConvertStringToArray(str);
      List<LevelRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split('@');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogError($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogError($"Invalid rarity weight: {rarityData[1]}");
          continue;
        }

        SelectableLevel level = StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName == rarityData[0]);

        if (level == null)
        {
          Plugin.logger.LogError($"Invalid level name: {rarityData[0]}");
          continue;
        }

        output.Add(new LevelRarity { Level = level, Weight = weight });
      }

      return output.ToArray();
    }

    public static WeatherRarity[] ConvertStringToWeatherWeights(string str)
    {
      // i want to use the following format:
      // Weather@Weight;Weather@Weight;Weather@Weight

      string[] rarities = ConvertStringToArray(str);
      List<WeatherRarity> output = [];

      foreach (string rarity in rarities)
      {
        string[] rarityData = rarity.Split('@');

        if (rarityData.Length != 2)
        {
          Plugin.logger.LogError($"Invalid rarity data: {rarity}");
          continue;
        }

        if (!int.TryParse(rarityData[1], out int weight))
        {
          Plugin.logger.LogError($"Invalid rarity weight: {rarityData[1]}");
          continue;
        }

        Weather weather = WeatherManager.Weathers.FirstOrDefault(w => w.name == rarityData[0]);

        if (weather == null)
        {
          Plugin.logger.LogError($"Invalid weather name: {rarityData[0]}");
          continue;
        }

        output.Add(new WeatherRarity { Weather = weather, Weight = weight });
      }

      return output.ToArray();
    }
  }
}
