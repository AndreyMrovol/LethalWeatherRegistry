using System.Collections.Generic;
using System.Reflection;
using MrovLib.Definitions;
using UnityEngine;
using WeatherRegistry.Definitions;
using WeatherRegistry.Managers;

namespace WeatherRegistry.Modules
{
  public static class HostTerminalCommands
  {
    public static TerminalKeyword WeatherVerb;
    public static TerminalNode CommandNode;

    public static Dictionary<string, WeatherCommandNode> RegisteredCommands = [];

    public static void Init(TerminalKeyword verb)
    {
      WeatherVerb = verb;
      CommandNode = TerminalNodeManager.CreateTerminalNode("Weather Commands");
      CommandNode.playSyncedClip = 3;

      WeatherCommandNode ChangeWeatherCommand = new("change") { Type = CommandType.Command, };
      RegisteredCommands.Add(ChangeWeatherCommand.Name, ChangeWeatherCommand);

      foreach (Weather weather in WeatherManager.Weathers)
      {
        WeatherCommandNode ChangeWeatherSubCommand =
          new(weather.GetAlphanumericName()) { CommandArgument = weather.GetAlphanumericName().ToLower(), };

        ChangeWeatherCommand.Subcommands.Add(ChangeWeatherSubCommand);
      }
    }

    public static TerminalNode RunWeatherCommand(TerminalNode node, string commandName, string subCommandName)
    {
      RegisteredCommands.TryGetValue(commandName, out var command);
      WeatherCommandNode subCommand = command.Subcommands.Find(sc => sc.Name == subCommandName);

      string result = subCommand.Execute();

      CommandNode.displayText = result;
      return CommandNode;
    }
  }
}
