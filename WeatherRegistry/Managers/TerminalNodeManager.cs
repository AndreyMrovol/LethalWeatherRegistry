using System.Collections.Generic;
using System.Linq;
using MrovLib;
using UnityEngine;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Managers
{
  public class TerminalNodeManager
  {
    internal static List<TerminalNode> ManagedTerminalNodes = [];

    internal static TerminalNode lastResolvedNode = null;

    internal static Dictionary<TerminalNode, SelectableLevel> ForecastTerminalNodes = [];

    public static void Init()
    {
      ManagedTerminalNodes = [];
      ForecastTerminalNodes = [];

      TerminalCommands.Init(AddVerb("Weather", "weather"));
    }

    public static TerminalKeyword AddVerb(string name, string word)
    {
      TerminalKeyword verb = ScriptableObject.CreateInstance<TerminalKeyword>();
      verb.name = name;
      verb.word = word;
      verb.isVerb = true;

      ContentManager.AddTerminalKeywords([verb]);
      return verb;
    }

    public static void AddTerminalContent(List<TerminalNode> terminalNodes = null, List<TerminalKeyword> terminalKeywords = null)
    {
      if (terminalNodes != null && terminalNodes.Count > 0)
      {
        ContentManager.AddTerminalNodes(terminalNodes);
      }

      if (terminalKeywords != null && terminalKeywords.Count > 0)
      {
        ContentManager.AddTerminalKeywords(terminalKeywords);
      }
    }

    public static TerminalNode CreateTerminalNode(string name, string terminalEvent = "")
    {
      TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

      terminalNode.name = name;
      terminalNode.terminalEvent = terminalEvent;

      terminalNode.displayText = "";

      terminalNode.clearPreviousText = true;
      terminalNode.acceptAnything = true;

      terminalNode.terminalOptions = [];

      terminalNode.maxCharactersToType = 25;
      terminalNode.itemCost = 0;

      terminalNode.buyItemIndex = -1;
      terminalNode.buyVehicleIndex = -1;
      terminalNode.buyRerouteToMoon = -1;
      terminalNode.displayPlanetInfo = -1;
      terminalNode.shipUnlockableID = -1;
      terminalNode.creatureFileID = -1;
      terminalNode.storyLogFileID = -1;
      terminalNode.playSyncedClip = -1;

      return terminalNode;
    }

    public static (TerminalNode node, TerminalKeyword keyword, CompatibleNoun noun) CreateCommandNode(
      TerminalKeyword verb,
      string word,
      string terminalEvent,
      bool addToTerminal = true
    )
    {
      TerminalNode commandNode = CreateTerminalNode($"{word}Node", terminalEvent);

      TerminalKeyword commandKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
      commandKeyword.word = word;
      commandKeyword.name = $"{word}Keyword";
      commandKeyword.defaultVerb = verb;

      CompatibleNoun commandNoun = new() { noun = commandKeyword, result = commandNode, };
      commandKeyword.compatibleNouns = [commandNoun];

      if (verb != null)
      {
        List<CompatibleNoun> verbNouns = verb.compatibleNouns?.ToList() ?? [];
        verbNouns.Add(commandNoun);
        verb.compatibleNouns = verbNouns.ToArray();
      }

      if (addToTerminal)
      {
        AddTerminalContent([commandNode], [commandKeyword]);
      }

      return (commandNode, commandKeyword, commandNoun);
    }
  }
}
