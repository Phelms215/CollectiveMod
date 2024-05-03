using System;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Managers;
using Collective.Utilities;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Systems.UI.ComputerTabs;

public class DistributorTab : UIParent
{
    private Guid? _activeShop = null;
    private GameObject? _navBar;
    private GameObject? _buttonParent = GameObject.Find("---GAME---/Computer/Screen/Market App/Taskbar/Buttons/");
    private ScrollRect? _scrollArea;

    private readonly GameObject _productsTab =
        GameObject.Find("---GAME---/Computer/Screen/Market App/Tabs/Products Tab");

    private readonly GameObject _furnitureTab =
        GameObject.Find("---GAME---/Computer/Screen/Market App/Tabs/Furnitures Tab");

    private readonly GameObject _cartButton =
        GameObject.Find("---GAME---/Computer/Screen/Market App/Taskbar/Cart Button");

    private void Awake()
    {
        DistributorTabLoad();
        BuildNavBar();
    }
 
    
    private void DistributorTabLoad()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("DistributorPanel", _productsTab.transform.parent);
        if (ThisObject == null) return;
        ThisObject.transform.localPosition = _productsTab.transform.localPosition;
        ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
            .font = UIUtility.GetPrimaryFont();
        _scrollArea = ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(1)
            .GetComponent<ScrollRect>();
        UpdateView();
    }
 

    public override void UpdateView()
    {
        if (_scrollArea == null) return;
        UIUtility.ClearScrollRect(_scrollArea);
        var manager = Collective.GetManager<DistributionManager>();
        var distributors = manager.Distributors;
        distributors.Sort((a, b) => a.MinLevel.CompareTo(b.MinLevel));
        distributors.ForEach(AddDistributor);
        _scrollArea.content.sizeDelta = new Vector2(_scrollArea.content.sizeDelta.x, 100 * distributors.Count);
        _scrollArea.verticalNormalizedPosition = 1.0f;
    }

    private void BuildNavBar()
    {
        if (_buttonParent == null)
        {
            Collective.Log.Error("Could not find the button parent");
            return;
        }

        // Disable the existing buttons & car 
        _buttonParent.SetActive(false);
        _buttonParent.transform.GetChild(0).gameObject.SetActive(false);
        _buttonParent.transform.GetChild(1).gameObject.SetActive(false);
        _cartButton.SetActive(false);

        // Load our prefab if it isn't already loaded
        _navBar ??= UIUtility.LoadAsset<GameObject>("MarketNavBar", _buttonParent.transform);
        if (_navBar == null) return;

        var holder = _navBar.transform.GetChild(0);
        if (holder == null) return;

        var goBack = holder.transform.GetChild(0);
        var storeInfo = holder.transform.GetChild(1);

        if (goBack == null) return;
        if (storeInfo == null) return;
        goBack.GetComponent<Button>().onClick.AddListener(NavGoBackButton);

        var selectedStoreName = Collective.GetManager<DistributionManager>().Distributors
            .FirstOrDefault(d => d.Id == _activeShop)
            ?.Name;
        if (selectedStoreName == null) return;
        storeInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedStoreName;
        storeInfo.gameObject.SetActive(true);
        _cartButton.SetActive(true);
    }

    private void NavGoBackButton()
    {
        _activeShop = null;
        _cartButton.SetActive(false);
        _furnitureTab.gameObject.SetActive(false);
        Collective.GetManager<DistributionManager>().RemoveActiveWebsite();
        Show();
    }


    public override void Show()
    {
        base.Show();
        BuildNavBar();
        if (_navBar != null)
            _navBar.transform.localPosition = new Vector3(-75, 0.5f, 0);
    }

    private void AddDistributor(Distributor distributor)
    {
        var parent = _scrollArea;
        if (parent == null) return;

        var button = UIUtility.LoadAsset<GameObject>("DistributorButton", parent.content);
        if (button == null) return;

        button.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = distributor.Name;
        button.transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            distributor.Description;

        if (distributor.JoinCost == 0 || distributor.IsMember)
            button.transform.GetChild(1).transform.GetChild(2).gameObject.SetActive(false);
        else
            button.transform.GetChild(1).transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                "One-Time Membership Fee $ " + distributor.JoinCost;

        button.transform.GetChild(2).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            "Level " + distributor.MinLevel + " Required";
        button.transform.GetChild(1).transform.GetChild(0).GetComponent<Image>().sprite =
            UIUtility.GetSprite(distributor.Icon);

        if (distributor.IsMember)
        {
            button.transform.GetChild(2).gameObject.SetActive(false);
            button.transform.GetChild(3).gameObject.SetActive(true);
            button.transform.GetChild(3).GetComponent<Button>().onClick
                .AddListener(() => VisitWebsite(distributor.Id));
            return;
        }

        if (UIUtility.GetStoreLevel() < distributor.MinLevel) return;
        button.transform.GetChild(2).gameObject.SetActive(false);
        button.transform.GetChild(4).gameObject.SetActive(true);


        button.transform.GetChild(4).GetComponent<Button>().onClick
            .AddListener(() => BuyMemberAccess(distributor));
    }


    private void BuyMemberAccess(Distributor distributor)
    {
        if (!Singleton<MoneyManager>.Instance.HasMoney(distributor.JoinCost))
        {
            Collective.GetManager<UIManager>().DisplayMessage("You don't have enough money to join this store! $ " +
                                                              distributor.JoinCost + " is required!");
            return;
        }

        Collective.Log.Info("Buying Distributor Access");
        Collective.GetManager<DistributionManager>().UnlockDistributor(distributor.Id);
        UpdateView();
    }

    private void VisitWebsite(Guid distributorId)
    {
        _activeShop = distributorId;
        BuildNavBar();

        
        if (_buttonParent != null)
            _buttonParent.SetActive(true);

        var thisDistributor = Collective.GetManager<DistributionManager>().Distributors.FirstOrDefault(d => d.Id == distributorId);

        if (thisDistributor == null)
        {
            Collective.Log.Error("Could not find distributor with id: " + distributorId);
            return;
        }
        
        if (thisDistributor.Type == DistributorType.Furniture)
        {
            var productViewer = GameObject.FindObjectOfType<ProductViewer>();
            var cart = productViewer.m_ShoppingCart;
            cart.m_CartItems.ToList().ForEach(cartItem => cartItem.RemoveProductFromCart());
            
            Collective.GetManager<DistributionManager>().RemoveActiveWebsite();
            _productsTab.gameObject.SetActive(false);
            _furnitureTab.gameObject.SetActive(true);
        }
        else
        {
            _productsTab.gameObject.SetActive(true);
            _furnitureTab.gameObject.SetActive(false);
            thisDistributor.VisitWebsite();
        }
        Hide();
    }
}