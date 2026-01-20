using System.Collections.Generic;

namespace WeatherRegistry.Definitions
{
  public abstract class CommandNode(string Name) : MrovLib.Definitions.CommandNode(Name)
  {
    public new List<WeatherCommandNode> Subcommands { get; set; } = [];

    public virtual string Execute()
    {
      return "";
    }
  }

  public class WeatherCommandNode(string Name) : CommandNode(Name)
  {
    public override string Execute()
    {
      SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
      Weather weather = ConfigHelper.ResolveStringToWeather(this.CommandArgument);

      if (weather == null)
      {
        return $"Weather '{this.CommandArgument}' not found.";
      }

      WeatherController.ChangeWeather(currentLevel, weather);

      return $"Changed weather to {weather.Name} on level {currentLevel.PlanetName}";
    }
  }
}
