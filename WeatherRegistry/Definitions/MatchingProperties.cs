using System;
using UnityEngine;

namespace WeatherRegistry.Definitions
{
  [Serializable]
  public class MatchingProperties
  {
    [field: Header("Matching Properties")]
    [field: SerializeField]
    [field: Tooltip("Name of the weather to match with")]
    public string Name { get; set; }

    [field: SerializeField]
    [field: Tooltip("Should Registry allow that weather to happen on this level?")]
    public bool Allow { get; set; } = true;

    [field: SerializeField]
    [field: Tooltip("Should Registry allow this setting to be user-overridable (i.e. in the configs)?")]
    public bool Overridable { get; set; }

    public virtual void Reset()
    {
      Name = "";
      Allow = true;
      Overridable = false;
    }
  }

  [Serializable]
  public class WeatherMatchingProperties : MatchingProperties
  {
    [field: Space(5)]
    [field: Header("Matched Weather Properties")]
    [field: SerializeField]
    [field: Range(0, 10000)]
    public int DefaultWeight { get; set; }

    public new void Reset()
    {
      base.Reset();
      DefaultWeight = 100;
    }
  }

  public class WeatherMatcher : MonoBehaviour
  {
    [field: SerializeField]
    public SelectableLevel Level { get; set; }

    [field: SerializeField]
    public WeatherMatchingProperties[] Weathers { get; set; } = [];

    public WeatherMatcher()
    {
      WeatherManager.WeatherMatchers.Add(this);
    }

    public void Reset()
    {
      Weathers = [];
    }
  }
}
