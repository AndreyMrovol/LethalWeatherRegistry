using System.Collections.Generic;

namespace WeatherRegistry.Definitions
{
  public abstract class RegistryCommandNode(string Name) : MrovLib.Definitions.CommandNode(Name)
  {
    public new List<RegistryCommandNode> Subcommands { get; set; } = [];

    public int TerminalSound { get; set; } = -1;

    public virtual string Execute(string[] args)
    {
      return "";
    }
  }

  public class WeatherChangeCommandNode(string Name) : RegistryCommandNode(Name)
  {
    public override string Execute(string[] args)
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
