using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(SalesUIElement), "UpdateTotalPrice")]
public class SalesUIElementUpdateUnitPricePatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching 
    private static bool Prefix(SalesUIElement __instance)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return true;
        var perSaleCost = Collective.GetManager<DistributionManager>().PerSaleCost(__instance.m_ProductID);
        __instance.m_TotalPrice = perSaleCost * int.Parse(__instance.m_ItemCountInput.text);
        __instance.m_TotalPriceText.text = __instance.m_TotalPrice.ToMoneyText(__instance.m_TotalPriceText.fontSize);
        return false;
    }
}