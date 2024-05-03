using Collective.Components.Modals;
using Collective.Systems.Managers;
using HarmonyLib;
using MyBox;

namespace Collective.Patches;

[HarmonyPatch(typeof(StoreStatusSign), "InstantInteract")]
public class StoreStatusSignPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(ref bool __result)
    {
        if (Singleton<StoreStatus>.Instance.IsOpen)
        {
            return true;
        }

        if (Collective.GetManager<PermitManager>().ValidateCurrentHour())
        {
            return true;
        }

        Collective.GetManager<UIManager>().DisplayMessage("Not within authorized operating hours.");
        return false;
    }

}