using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(CartItem), "Setup")]
public class CartItemUpdateUnitPricePatch
{
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static void Postfix(CartItem __instance)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return;
        var perSaleCost = Collective.GetManager<DistributionManager>().PerSaleCost(__instance.m_ProductID);
        __instance.m_UnitPriceText.text = perSaleCost.ToMoneyText(__instance.m_UnitPriceText.fontSize); 
    }
}