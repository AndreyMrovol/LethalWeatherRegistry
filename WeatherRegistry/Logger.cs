using System;
using BepInEx.Logging;

namespace WeatherRegistry
{
  [Flags]
  public enum LoggingType
  {
    Basic,
    Debug,
    Developer,
  }

  public class Logger
  {
    private readonly string _name;
    private readonly ManualLogSource _logSource = BepInEx.Logging.Logger.CreateLogSource($"WeatherRegistry");

    private readonly LoggingType _defaultLoggingType;

    public Logger(string SourceName, LoggingType defaultLoggingType = LoggingType.Basic)
    {
      _defaultLoggingType = defaultLoggingType;
      _name = SourceName;
    }

    public string CustomName { get; set; } = string.Empty;

    public void LogCustom(string data, LogLevel level, LoggingType type)
    {
      if (ConfigManager.LoggingLevels.Value.HasFlag(type))
      {
        _logSource.Log(level, $"[{_name}] {data}");
      }
    }

    public void Log(LogLevel level, string data)
    {
      if (ConfigManager.LoggingLevels.Value.HasFlag(_defaultLoggingType))
      {
        _logSource.Log(level, $"[{_name}] {data}");
      }
    }

    public void LogInfo(string data)
    {
      Log(LogLevel.Info, data);
    }

    public void LogWarning(string data)
    {
      Log(LogLevel.Warning, data);
    }

    public void LogError(string data)
    {
      Log(LogLevel.Error, data);
    }

    public void LogDebug(string data)
    {
      Log(LogLevel.Debug, data);
    }

    public void LogFatal(string data)
    {
      Log(LogLevel.Fatal, data);
    }

    public void LogMessage(string data)
    {
      Log(LogLevel.Message, data);
    }
  }
}
