using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(MarketShoppingCart), "CurrentShippingCost",  MethodType.Getter)]
public class MarketShoppingCurrentShippingCostPatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static void Postfix( ref float __result)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return;
        __result  = Collective.GetManager<DistributionManager>().CalculateShippingCost();
    }
}