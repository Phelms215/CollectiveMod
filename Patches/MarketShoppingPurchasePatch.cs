using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(MarketShoppingCart), "Purchase")]
public class MarketShoppingPurchasePatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(MarketShoppingCart __instance)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return true;
        __instance.m_OrderTotalPrice = 0.0f;
        var cartList = __instance.m_CartData.ProductInCarts;
        var purchase = Collective.GetManager<DistributionManager>().PurchaseCart(cartList);
        if (!purchase) return false;
        __instance.CleanCart();
        __instance.m_CartWindow.SetActive(false);
        return false;
    }
}