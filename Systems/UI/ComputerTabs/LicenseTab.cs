using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Managers;
using Collective.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Systems.UI.ComputerTabs;

public class LicenseTab : UIParent
{ 
    private void Awake() => LicensePanelLoad();

    private void LicensePanelLoad()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("PermitPanel", UIUtility.TabsObject().transform);
        if (ThisObject == null) return;
        ThisObject.transform.localPosition = UIUtility.TabsObject().transform.GetChild(1).localPosition;

        var font = UIUtility.GetPrimaryFont();
        ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().font = font;
        ThisObject.transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().font = font;
        UpdateView();
    }

    public override void UpdateView()
    {
        if (ThisObject == null) return;
        var permitManager = Collective.GetManager<PermitManager>();
        var storeHourPanel = ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(1)
            .GetComponent<ScrollRect>();
        var productPanel = ThisObject.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1)
            .GetComponent<ScrollRect>();
        UIUtility.ClearScrollRect(storeHourPanel);
        UIUtility.ClearScrollRect(productPanel);
        
        // Store Hour (And eventually other store/town related stuff (delivery hours)
        var storeHourPermits = permitManager.GetPermits(PermitType.StoreHours);
        storeHourPermits.Sort((a, b) => a.Level.CompareTo(b.Level)); 
        storeHourPermits.ForEach(permit => AddPermit(permit, storeHourPanel));
        storeHourPanel.content.sizeDelta =
            new Vector2(storeHourPanel.content.sizeDelta.x, 111 * storeHourPermits.Count);
        storeHourPanel.verticalNormalizedPosition = 1.0f;
        
        // Permits to unlock specific products
        var permitList = permitManager.GetPermits(PermitType.Products); 
        permitList.Sort((a, b) => a.Level.CompareTo(b.Level));
        permitList.ForEach(permit => AddPermit(permit, productPanel));
        productPanel.content.sizeDelta = new Vector2(productPanel.content.sizeDelta.x, 111 * storeHourPermits.Count);
        productPanel.verticalNormalizedPosition = 1.0f;
    }

    private void AddPermit(Permit permit, ScrollRect parent)
    {
        var button = UIUtility.LoadAsset<GameObject>("PermitButton", parent.content);
        if(button == null) return;
        button.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = permit.Title;
        button.transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = permit.Description;
        button.transform.GetChild(1).transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
            "Cost $ " + permit.Cost;
        button.transform.GetChild(2).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            "Level " + permit.Level + " Required";

        if (Collective.GetManager<PermitManager>().IsUnlocked(permit.ID))
        {
            button.transform.GetChild(2).gameObject.SetActive(false);
            button.transform.GetChild(3).gameObject.SetActive(true);
            return;
        }

        if (UIUtility.GetStoreLevel() < permit.Level) return;
        button.transform.GetChild(2).gameObject.SetActive(false);
        button.transform.GetChild(4).gameObject.SetActive(true);
        button.transform.GetChild(4).GetComponent<Button>().onClick
            .AddListener(() => PurchaseButtonWasClicked(permit.ID));

    }

    private void PurchaseButtonWasClicked(string permitId)
    { 
        Collective.GetManager<PermitManager>().UnlockPermit(permitId);
        UpdateView();
    }

}