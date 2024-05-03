using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(SalesItem), "AddToCart")]
public class SalesItemAddToCartPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(SalesItem __instance)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return true;
        var productInfo = Collective.GetManager<DistributionManager>().ProductInfos
            .FirstOrDefault(i => i.ID == __instance.m_ProductID);
        if (productInfo == null) return false;
        if (Collective.GetManager<PermitManager>().CanBuyProduct(productInfo)) return AddToCard(__instance);
        Collective.GetManager<UIManager>().ShowMessage("Cannot Buy Product","You do not have the required permits to sell this product.");
        return false;
    }

    private static bool AddToCard(SalesItem instance)
    {
        var itemQuantity = new ItemQuantity()
        {
            Products = new Dictionary<int, int>((IDictionary<int, int>)instance.m_ProductQuantity.Products)
        };
        instance.m_ProductViewer.ShoppingCart.AddProduct(itemQuantity, SalesType.PRODUCT);
        return false;
    }
    
}