using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using WeatherRegistry.Managers;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
  class TerminalLoadNewNodePatch
  {
    [HarmonyPrefix]
    public static bool GameMethodPatch(Terminal __instance, ref TerminalNode node)
    {
      if (TerminalNodeManager.ForecastTerminalNodes.Keys.Contains(node))
      {
        Plugin.debugLogger.LogDebug($"Forecast for node {node.name} requested");

        string displayText = Forecasts.GetForecast(TerminalNodeManager.ForecastTerminalNodes[node]);
        __instance.currentText = displayText;

        return false;
      }
      else
      {
        return true;
      }
    }
  }
}
