using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  public class NetcodePatcher
  {
    internal static void PatchNetcodeMethods()
    {
      Type[] types;
      try
      {
        types = Assembly.GetExecutingAssembly().GetTypes();
      }
      catch (ReflectionTypeLoadException e)
      {
        types = [.. e.Types.Where(type => type != null)];
      }

      foreach (Type type in types)
      {
        foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
          if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
          {
            _ = method.Invoke(null, null);
          }
        }
      }
    }
  }
}
