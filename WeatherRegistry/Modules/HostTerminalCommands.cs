using System.Collections.Generic;
using MrovLib.Definitions;
using WeatherRegistry.Definitions;
using WeatherRegistry.Managers;
using static WeatherRegistry.Modules.Forecasts;

namespace WeatherRegistry.Modules
{
  public static class HostTerminalCommands
  {
    public static TerminalKeyword WeatherVerb;
    public static TerminalNode CommandNode;

    public static Dictionary<string, RegistryCommandNode> RegisteredCommands = [];

    public static void Init(TerminalKeyword verb)
    {
      WeatherVerb = verb;
      CommandNode = TerminalNodeManager.CreateTerminalNode("Weather Commands");
      CommandNode.acceptAnything = false;

      WeatherChangeCommandNode ChangeWeatherCommand = new("change") { Type = CommandType.Command, TerminalSound = 3 };
      RegisteredCommands.Add(ChangeWeatherCommand.Name, ChangeWeatherCommand);

      foreach (Weather weather in WeatherManager.Weathers)
      {
        WeatherChangeCommandNode ChangeWeatherSubCommand =
          new(weather.GetAlphanumericName()) { CommandArgument = weather.GetAlphanumericName().ToLower(), TerminalSound = 3, };

        ChangeWeatherCommand.Subcommands.Add(ChangeWeatherSubCommand);
      }

      WeatherForecastCommandNode ForecastWeatherCommand = new("forecast") { Type = CommandType.Command };
      HostTerminalCommands.RegisteredCommands.Add(ForecastWeatherCommand.Name, ForecastWeatherCommand);

      foreach (SelectableLevel level in MrovLib.LevelHelper.Levels)
      {
        WeatherForecastCommandNode ChangeWeatherSubCommand =
          new(ConfigHelper.GetAlphanumericName(level)) { CommandArgument = ConfigHelper.GetAlphanumericName(level).ToLower(), };

        ForecastWeatherCommand.Subcommands.Add(ChangeWeatherSubCommand);
      }
    }

    public static TerminalNode RunWeatherCommand(string commandName, string[] args)
    {
      RegisteredCommands.TryGetValue(commandName, out var command);
      string result = "";

      if (command == null)
      {
        CommandNode.displayText = $"Command '{commandName}' not found.";
        return CommandNode;
      }

      // run the command itself when
      // there's no subcommands OR there's no subcommand specified

      if (command.Subcommands.Count == 0 || args.Length < 1)
      {
        result = command.Execute(args);
      }
      else
      {
        var commandname = args[0];
        Plugin.debugLogger.LogInfo($"Running subcommand '{commandname}' - is null? {commandname == null}");

        Plugin.debugLogger.LogInfo(
          $"Looking for subcommand '{args[0]}'; available: {string.Join(", ", command.Subcommands.ConvertAll(sc => sc.Name))}"
        );

        try
        {
          RegistryCommandNode subCommand = command.Subcommands.Find(sc => sc.Name == args[0]);

          if (subCommand == null)
          {
            result = $"Subcommand '{args[0]}' not found for command '{commandName}'.";
          }

          result = subCommand.Execute(args);
        }
        catch (System.Exception e)
        {
          Plugin.debugLogger.LogError($"Error finding subcommand: {e}");
        }
      }

      CommandNode.displayText = result;
      return CommandNode;
    }
  }
}
