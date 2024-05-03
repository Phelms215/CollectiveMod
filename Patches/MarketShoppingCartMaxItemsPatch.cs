using HarmonyLib; 

namespace Collective.Patches;
 
[HarmonyPatch(typeof(MarketShoppingCart), "CartMaxed")]
public class MarketShoppingCartMaxItemsPatch
{

    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static void Postfix(ref bool __result)
    {
        __result = false;
    }
}