using System;
using System.Collections.Generic;
using System.Linq;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;

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
