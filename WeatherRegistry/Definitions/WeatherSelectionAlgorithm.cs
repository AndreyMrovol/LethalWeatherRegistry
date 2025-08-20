using System.Collections.Generic;

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
      return new(startOfRound.randomMapSeed + 31);
    }

    public virtual Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
    {
      return [];
    }
  }
}
