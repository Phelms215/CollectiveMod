using Collective.Systems.Managers;
using HarmonyLib;
using UnityEngine;

namespace Collective.Patches;
 

    [HarmonyPatch(typeof(Cashier), "ScanAnimation")]
    public class ScanPatch
    {
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        // name convention required for harmony patching
        private static bool Prefix(Cashier __instance)
        {  
            Collective.GetManager<StaffManager>().CompleteAnimation(__instance, Animator.StringToHash("Scan"));
            return false;
        }
    
    }
