using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib; 

[HarmonyPatch(typeof(MarketShoppingCart), "UpdateTotalPrice")]
public class MarketShoppingUpdateTotalPricePatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(MarketShoppingCart __instance)
    {
        if (Collective.GetManager<DistributionManager>().IsFurnitureStore()) return true;
        __instance.m_OrderTotalPrice = 0.0f;
        var cartList = __instance.m_CartData.ProductInCarts;
        var total = Collective.GetManager<DistributionManager>().CalculateCartTotal(cartList); 
        __instance.m_OrderTotalPrice = total;  
        __instance.UpdateUI(Singleton<MoneyManager>.Instance.HasMoney(total + __instance.CurrentShippingCost));
        return false;
    }
}