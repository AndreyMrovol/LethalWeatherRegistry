using UnityEngine;

namespace WeatherRegistry.Utils
{
  public static class InstanceCreator
  {
    public static T CreateInstance<T>()
      where T : ScriptableObject
    {
      T instance = ScriptableObject.CreateInstance<T>();
      instance.hideFlags = HideFlags.HideAndDontSave;
      return instance;
    }
  }
}
