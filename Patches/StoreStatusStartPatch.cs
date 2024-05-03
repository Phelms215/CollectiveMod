using Collective.Systems.Managers;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(StoreStatus), "Start")]
public class StoreStatusStartPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(StoreStatus __instance)
    {
        return false;
    }
}
