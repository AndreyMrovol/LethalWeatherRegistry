namespace WeatherRegistry
{
  public class WeatherResolvable
  {
    public virtual string WeatherName { get; }
    public virtual LevelWeatherType WeatherType { get; }

    public override string ToString()
    {
      return $"{WeatherName} ({(int)WeatherType})";
    }
  }

  public class WeatherNameResolvable(string weatherName) : WeatherResolvable
  {
    public override string WeatherName => weatherName;

    public override LevelWeatherType WeatherType
    {
      get
      {
        if (WeatherRegistry.WeatherManager.IsSetupFinished)
        {
          Weather resolvedWeather = WeatherRegistry.ConfigHelper.ResolveStringToWeather(weatherName);

          return resolvedWeather == null ? LevelWeatherType.None : resolvedWeather.VanillaWeatherType;
        }
        else
        {
          Plugin.debugLogger.LogDebug($"Tried to resolve weather name {weatherName} before setup finished");
          return LevelWeatherType.None;
        }
      }
    }
  }

  public class WeatherTypeResolvable(LevelWeatherType weatherType) : WeatherResolvable
  {
    public override LevelWeatherType WeatherType => weatherType;

    public override string WeatherName
    {
      get
      {
        if (WeatherRegistry.WeatherManager.IsSetupFinished)
        {
          return WeatherRegistry.WeatherManager.GetWeather(weatherType).Name;
        }
        else
        {
          return "";
        }
      }
    }
  }
}
