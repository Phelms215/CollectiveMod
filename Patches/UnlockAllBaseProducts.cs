using HarmonyLib;
using Collective.Systems.Managers;

namespace Collective.Patches;


[HarmonyPatch(typeof(ProductLicenseManager), "IsLicenseUnlocked")]
public class UnlockAllBaseProductsPatch
{

    [HarmonyPrefix] 
    // ReSharper disable once RedundantAssignment
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix(ref bool __result)
    {
        __result = true; // Special harmony variable to ensure they return true 
        return false; // Skip the original method
    }
}