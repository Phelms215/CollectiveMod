using Collective.Systems.Managers;
using HarmonyLib;
using UnityEngine;

namespace Collective.Patches;
 


    [HarmonyPatch(typeof(Cashier), "TakePaymentAnimation")]
    public class CashierPaymentPatch
    {
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        // name convention required for harmony patching
        private static bool Prefix(Cashier __instance)
        { 
            // Override the cashier animation - sending to my guy instead
            Collective.GetManager<StaffManager>().CompleteAnimation(__instance, Animator.StringToHash("Take Payment"));
            return false;
        }
    
    }
