using System.Collections.Generic;

namespace WeatherRegistry.Managers
{
  public class TerminalNodeManager
  {
    internal static List<TerminalNode> ManagedTerminalNodes = [];
    internal static Dictionary<TerminalNode, SelectableLevel> ForecastTerminalNodes = [];
  }
}
