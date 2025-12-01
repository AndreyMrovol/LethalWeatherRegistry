using System.Collections.Generic;
using WeatherRegistry.Modules;

namespace WeatherRegistry.Definitions
{
  public class WeatherSelectionAlgorithm
  {
    public virtual Logger Logger
    {
      get => WeatherCalculation.Logger;
    }

    public virtual System.Random GetRandom(StartOfRound startOfRound)
    {
      return new(startOfRound.randomMapSeed + 35);
    }

    public virtual Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
    {
      return [];
    }
  }
}
