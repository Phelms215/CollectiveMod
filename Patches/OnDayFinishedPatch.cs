using System;
using System.Collections;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(DayCycleManager))]
[HarmonyPatch("FinishTheDay")]
public class OnDayFinishedPatch
{

    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(DayCycleManager __instance)
    {
        return false;
    }

}