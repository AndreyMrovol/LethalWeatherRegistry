using System;
using BepInEx.Logging;
using MrovLib;

namespace WeatherRegistry
{
  public class Logger : MrovLib.Logger
  {
    private new readonly ManualLogSource _logSource = BepInEx.Logging.Logger.CreateLogSource($"WeatherRegistry");
    private new LoggingType _defaultLoggingType;

    public Logger(string SourceName, LoggingType defaultLoggingType = LoggingType.Basic)
      : base(SourceName, MrovLib.LoggingType.Basic)
    {
      _defaultLoggingType = defaultLoggingType;
      _name = SourceName;
    }

    public override bool ShouldLog(LoggingType type)
    {
      return ConfigManager.LoggingLevels.Value >= type;
    }
  }
}
