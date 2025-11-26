using System.Collections.Generic;

namespace WeatherRegistry.Utils
{
  public class WeightHandler<T, OT> : MrovLib.WeightHandler<T>
  {
    private Dictionary<T, OT> originDict = [];

    public OT GetOrigin(T item)
    {
      return originDict.ContainsKey(item) ? originDict[item] : default;
    }
  }
}
