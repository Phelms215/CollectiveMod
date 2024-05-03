using System;
using System.Collections;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(OnboardingManager), "CheckForLightsTutorialTime")]
public class OnboardingManagerDisableLightTutorialPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static bool Prefix()
    {
        return false;
    }
}