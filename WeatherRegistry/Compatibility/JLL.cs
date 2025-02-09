using JLL.Components;

namespace WeatherRegistry.Compatibility
{
  internal class JLLCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public bool IsJLLDoingWeatherOverride()
    {
      return GetJLLData().isDoingOverride;
    }

    public (bool isDoingOverride, WeatherEffect effect) GetJLLData()
    {
      if (JWeatherOverride.Instance != null)
      {
        Weather currentWeather = WeatherManager.GetCurrentLevelWeather();

        if (currentWeather == WeatherManager.NoneWeather)
        {
          return (false, null);
        }

        string effectName = TimeOfDay.Instance.effects[(int)currentWeather.VanillaWeatherType].name;

        WeatherEffect effect = JWeatherOverride.Instance.getOverrideEffect(effectName);
        if (effect != null)
        {
          return (true, effect);
        }
      }

      return (false, null);
    }
  }
}
