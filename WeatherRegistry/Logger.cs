using BepInEx.Configuration;
using BepInEx.Logging;

namespace WeatherRegistry
{
  public class Logger
  {
    private static ManualLogSource LogSource;
    private static ConfigEntry<bool> Enabled;

    // public Logger(string name, ConfigEntry<bool> enabled)
    // {
    //   LogSource = BepInEx.Logging.Logger.CreateLogSource(name);
    //   Enabled = enabled;
    // }
    // constructor

    public Logger(string name, ConfigEntry<bool> enabled)
    {
      LogSource = BepInEx.Logging.Logger.CreateLogSource(name);
      Enabled = enabled;
    }

    // this is overengineered to fuck
    // but so am i
    public static void Log(LogLevel level, object data)
    {
      if (Enabled.Value)
      {
        LogSource.Log(level, data);
      }
    }

    public void LogInfo(object data)
    {
      Log(LogLevel.Info, data);
    }

    public void LogWarning(object data)
    {
      Log(LogLevel.Warning, data);
    }

    public void LogError(object data)
    {
      Log(LogLevel.Error, data);
    }

    public void LogDebug(object data)
    {
      Log(LogLevel.Debug, data);
    }

    public void LogFatal(object data)
    {
      Log(LogLevel.Fatal, data);
    }

    public void LogMessage(object data)
    {
      Log(LogLevel.Message, data);
    }
  }
}
