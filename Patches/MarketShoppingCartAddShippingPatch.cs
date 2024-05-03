using System;
using System.Collections;
using Collective.Components.Definitions;
using Collective.Systems.Managers;
using Collective.Utilities;
using MyBox;
using TMPro;
using UnityEngine.UI;

namespace Collective.Patches;
 
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(MarketShoppingCart), "Initialize")]
public class MarketShoppingCartAddShippingPatch
{

    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    // name convention required for harmony patching
    private static void Postfix(MarketShoppingCart __instance)
    {
        var parent = __instance.m_BalanceText.transform.parent;



        var shippingTextInfo = new GameObject("Collective-ShippingOptionLabel").AddComponent<TextMeshProUGUI>();
        shippingTextInfo.fontSize = 8;
        shippingTextInfo.font = __instance.m_BalanceText.font;
        shippingTextInfo.color = Color.white;
        shippingTextInfo.transform.SetParent(parent, false);
        shippingTextInfo.transform.localPosition = new Vector3(15, 5, 0);
        shippingTextInfo.text = "Shipping Options";

        CreateDropDown(parent, __instance);

    }

 private static void CreateDropDown(Transform parent, MarketShoppingCart instance)
{
    var currentShipping = Collective.GetManager<DistributionManager>().GetShippingOption();
    var dropdownObject = UIUtility.LoadAsset<GameObject>("ShippingDropdown");
    dropdownObject.transform.SetParent(parent, false);
    dropdownObject.transform.localPosition = new Vector3(-54, 0, 0);
    var dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
    dropdown.onValueChanged.AddListener((value) =>
    {
        var option = value switch
        {
            0 => ShippingOptions.SameDay,
            1 => ShippingOptions.NextDay,
            _ => ShippingOptions.TwoDays
        };
        Collective.GetManager<DistributionManager>().UpdateShippingOption(option);
        instance.UpdateTotalPrice();
    });

    // Set initial value without firing the event
    dropdown.SetValueWithoutNotify((int)currentShipping);
}

}