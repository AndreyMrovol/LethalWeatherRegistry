using System.Linq;
using HarmonyLib;
using WeatherRegistry.Managers;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
  class TerminalRunTerminalEventsPatch
  {
    [HarmonyPrefix]
    public static bool GameMethodPatch(Terminal __instance, ref TerminalNode node)
    {
      if (TerminalNodeManager.ForecastTerminalNodes.Keys.Contains(node))
      {
        Plugin.debugLogger.LogDebug($"Forecast for node {node.name} requested");
        TerminalNodeManager.lastResolvedNode = node;

        string displayText = Forecasts.GetForecast(TerminalNodeManager.ForecastTerminalNodes[node]);
        __instance.currentText = displayText;

        return false;
      }
      // this comment is here so my formatter doesn't squash every case together cause it's less readable
      else
      {
        return true;
      }
    }
  }
}
