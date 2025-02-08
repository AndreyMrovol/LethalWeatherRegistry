using System.Collections.Generic;
using System.Linq;
using WeatherRegistry.Definitions;

namespace WeatherRegistry.Networking
{
  public class WeatherEffectData
  {
    internal static void EffectsReceived(WeatherEffectDataWrapper previousValue, WeatherEffectDataWrapper newValue)
    {
      Plugin.logger.LogDebug($"Effects received: [{string.Join("; ", newValue.Effects)}]");

      ApplyWeatherEffects(newValue.Effects);
    }

    internal static void ApplyWeatherEffects(LevelWeatherType[] weatherType)
    {
      WeatherEffectController.SetWeatherEffects(weatherType);
    }
  }
}
