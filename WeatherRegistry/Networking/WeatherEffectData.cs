using System.Collections.Generic;
using System.Linq;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Networking
{
  public class WeatherEffectData
  {
    internal static void EffectsReceived(WeatherEffectSyncData previousValue, WeatherEffectSyncData newValue)
    {
      Plugin.logger.LogDebug($"Effects received: [{string.Join("; ", newValue.WeatherType)}]");

      ApplyWeatherEffects(newValue.WeatherType);
    }

    internal static void ApplyWeatherEffects(LevelWeatherType weatherType)
    {
      WeatherEffectController.SetWeatherEffects(weatherType);
    }
  }
}
