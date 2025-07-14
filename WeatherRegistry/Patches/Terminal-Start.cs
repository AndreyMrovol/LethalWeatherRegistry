using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConsoleTables;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace WeatherRegistry.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TerminalStartPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    [HarmonyPriority(Priority.First)]
    public static void Postfix(Terminal __instance)
    {
      Startup.Init(__instance);
    }
  }
}
