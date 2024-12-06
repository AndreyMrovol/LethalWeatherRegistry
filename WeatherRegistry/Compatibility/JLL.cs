using JLL.Components;

namespace WeatherRegistry.Compatibility
{
  internal class JLLCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public bool IsJLLDoingWeatherOverride()
    {
      if (JWeatherOverride.Instance != null)
      {
        Weather currentWeather = WeatherManager.GetCurrentLevelWeather();
        string effectName = TimeOfDay.Instance.effects[(int)currentWeather.VanillaWeatherType].name;

        WeatherEffect effect = JWeatherOverride.Instance.getOverrideEffect(effectName);
        if (effect != null)
        {
          return true;
        }
      }

      return false;
    }
  }
}
