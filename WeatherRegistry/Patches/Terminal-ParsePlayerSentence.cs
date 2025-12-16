using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using WeatherRegistry.Managers;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
  class TerminalParsePlayerSentencePatch
  {
    [HarmonyPostfix]
    public static void GameMethodPatch(Terminal __instance, ref TerminalNode __result)
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

          if (words.Count >= 3)
          {
            string command = words[1];
            string weatherName = words[2];

            TerminalNode result = HostTerminalCommands.RunWeatherCommand(__result, command, weatherName);

            __result = result;
          }
        }
      }

      return;
    }
  }
}
