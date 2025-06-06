using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
  class TerminalLoadNewNodePatch
  {
    [HarmonyPrefix]
    public static bool GameMethodPatch(Terminal __instance, ref TerminalNode node)
    {
      if (Forecasts.ForecastTerminalNodes.Keys.Contains(node))
      {
        Plugin.debugLogger.LogDebug($"Forecast for node {node.name} requested");

        string displayText = Forecasts.GetForecast(Forecasts.ForecastTerminalNodes[node]);
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
