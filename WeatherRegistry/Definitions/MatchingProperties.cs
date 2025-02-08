using System;
using UnityEngine;

namespace WeatherRegistry.Definitions
{
  [Serializable]
  public class MatchingProperties
  {
    [field: Header("Matching Properties")]
    [field: SerializeField]
    [field: Tooltip("Name of the object to match with")]
    public string Name { get; set; }

    [field: SerializeField]
    [field: Tooltip("Should Registry allow this object to be matched with?")]
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

  [Serializable]
  public class LevelMatchingProperties : MatchingProperties
  {
    [field: Space(5)]
    [field: Header("Matched Levels Properties")]
    [field: SerializeField]
    [field: Range(0, 10000)]
    public int DefaultWeight { get; set; }

    public new void Reset()
    {
      base.Reset();
      DefaultWeight = 100;
    }
  }

  [Serializable]
  [CreateAssetMenu(fileName = "WeatherMatcher", menuName = "WeatherRegistry/WeatherMatcher", order = 5)]
  public class WeatherMatcher : ScriptableObject
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

  [Serializable]
  [CreateAssetMenu(fileName = "LevelMatcher", menuName = "WeatherRegistry/LevelMatcher", order = 5)]
  public class LevelMatcher : ScriptableObject
  {
    [field: SerializeField]
    public Weather Weather { get; set; }

    [field: SerializeField]
    public LevelMatchingProperties[] Levels { get; set; } = [];

    public LevelMatcher()
    {
      WeatherManager.LevelMatchers.Add(this);
    }

    public void Reset()
    {
      Levels = [];
    }
  }
}
