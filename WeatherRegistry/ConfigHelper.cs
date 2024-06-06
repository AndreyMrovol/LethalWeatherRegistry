using System.Collections.Generic;
using System.Linq;

namespace WeatherRegistry
{
  public class Rarity
  {
    public string Name { get; set; }
    public int Weight { get; set; }
  }

  internal class ConfigHelper
  {
    public static string GetNumberlessName(SelectableLevel level)
    {
      return new string(level.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
    }

    // convert string to array of strings
    public static string[] ConvertStringToArray(string str)
    {
      string[] output = str.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

      return output;
    }

    public static SelectableLevel[] ConvertStringToLevels(string str)
    {
      string[] levelNames = ConvertStringToArray(str);
      Dictionary<string, SelectableLevel> Levels = StartOfRound.Instance.levels.ToDictionary(level => GetNumberlessName(level), level => level);
      List<SelectableLevel> output = [];

      foreach (string level in levelNames)
      {
        SelectableLevel selectableLevel = Levels.GetValueOrDefault(level);

        Plugin.logger.LogDebug($"Selectable level: {selectableLevel}");

        output.Add(selectableLevel);
      }

      return output.ToArray();
    }

    public Rarity[] ConvertStringToRarities(string str)
    {
      string[] rarities = ConvertStringToArray(str);
      List<Rarity> output = [];

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

        output.Add(new Rarity { Name = rarityData[0], Weight = weight });
      }

      return output.ToArray();
    }
  }
}
