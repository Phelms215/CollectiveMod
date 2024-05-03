using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(MarketShoppingCart), "TimeCheck")]
public class MarketShoppingCartTimeCheckPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(MarketShoppingCart __instance)
    {
        return false;
    }
}
