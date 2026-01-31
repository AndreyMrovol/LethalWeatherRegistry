using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
  class TerminalParsePlayerSentencePatch
  {
    [HarmonyPrefix]
    public static bool GameMethodPatch(Terminal __instance, ref TerminalNode __result)
    {
      string input = __instance.screenText.text[^__instance.textAdded..]; // what the fuck?
      input = __instance.RemovePunctuation(input);

      List<string> words = input.Split(' ').ToList();

      if (words.Count >= 1)
      {
        // check if first word is a registered command

        if (words[0] == "weather")
        {
          // get the full command and pass it to the manager

          Plugin.debugLogger.LogWarning("Weather command detected, passing to WeatherCommandManager");

          // weather command arg1 arg2
          if (words.Count >= 2)
          {
            string command = words[1];
            string[] arguments = words.Skip(2).ToArray();

            TerminalNode result = HostTerminalCommands.RunWeatherCommand(command, arguments);

            __result = result;
            return false;
          }

          return true;
        }
      }

      return true;
    }
  }
}
