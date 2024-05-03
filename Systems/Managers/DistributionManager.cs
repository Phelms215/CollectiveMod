using System;
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Actions;
using Collective.Utilities;
using MyBox;
using Random = UnityEngine.Random;

namespace Collective.Systems.Managers;

public class DistributionManager : ManagerUtility, IManage, ITick
{
    public List<Distributor> Distributors { get; private set; } = new();
    public List<ProductInfo> ProductInfos { get; } = new();

    private StoreInventory _storeInventory = new();
    private Guid _activeWebsite = Guid.Empty;
    private ShippingOptions _selectedShippingOption = ShippingOptions.NextDay;
 
    public void UpdateInventory(StoreInventory storeInventory) => _storeInventory = storeInventory;

    public StoreInventory GetStoreInventory() => _storeInventory;

    public void UpdateShippingOption(ShippingOptions shippingOption) => _selectedShippingOption = shippingOption;
    
    public ShippingOptions GetShippingOption() => _selectedShippingOption;

    public bool IsFurnitureStore() => _activeWebsite == Guid.Empty;




    public void AddDistributor(Distributor distributor)
    {
        var existingDistributor = Distributors.FirstOrDefault(d => d.Id == distributor.Id);
        if (existingDistributor != null)
            Distributors.Remove(existingDistributor);
        Distributors.Add(distributor);
    }


    public void UnlockDistributor(Guid distributorId) 
    {
        var distributor = Distributors.FirstOrDefault(distributor => distributor.Id == distributorId);
        var unlockCost = distributor.JoinCost * -1;
        Singleton<MoneyManager>.Instance.MoneyTransition(unlockCost, MoneyManager.TransitionType.SUPPLY_COSTS);
        distributor.IsMember = true;
        distributor.RespectLevel = 1; 
    } 
    
    public void SetActiveWebsite(Guid guid, List<ItemQuantity> items)
    {
        if(_activeWebsite == guid) return;
        _activeWebsite = guid;
        items.ForEach(item =>
        {
            Collective.Log.Info("Item list contains qty" + item.FirstItemCount + " of " + item.FirstItemID);
        });
    }
    
    public void RemoveActiveWebsite() => _activeWebsite = Guid.Empty;

    public float PerSaleCost(int productId)
    {
        if (_activeWebsite == Guid.Empty) return 0f;
        var thisDist = Distributors.FirstOrDefault(distributor => distributor.Id == _activeWebsite);
        return thisDist?.PerSaleCost(productId) ?? 0f;
    }

    public float CalculateCartTotal(List<ItemQuantity> cartInventory)
    {
        if (_activeWebsite == Guid.Empty) return 0f;
        var thisDist = Distributors.FirstOrDefault(distributor => distributor.Id == _activeWebsite);
        return thisDist?.CalculateCartTotal(cartInventory) ?? 0f;
    }

    public float CalculateShippingCost()
    {
        if (_activeWebsite == Guid.Empty) return 0f;
        var thisDist = Distributors.FirstOrDefault(distributor => distributor.Id == _activeWebsite);
        if (thisDist == null) return 0f; 
        return thisDist.CalculateShippingCost(_selectedShippingOption);
    }
    public bool PurchaseCart(List<ItemQuantity> cartInventory)
    {
        if (_activeWebsite == Guid.Empty) return false;
        var thisDist = Distributors.FirstOrDefault(distributor => distributor.Id == _activeWebsite);
        if (thisDist == null) return false;
        
        var cartTotal = thisDist.CalculateCartTotal(cartInventory);
        var shippingCost = thisDist.CalculateShippingCost(_selectedShippingOption);

        
        var arrivalTime = new Hours(Random.Range(8,16), Random.Range(0,59));
        var arrivalDate = CalculateArrivalDate();
        if (_selectedShippingOption == ShippingOptions.SameDay)
        {
            var currentTime = Collective.GetNormalizedTime();
            if (currentTime.Hour > 12)
            {
                Collective.GetManager<UIManager>()
                    .ShowMessage("Order Cancelled","Too late to order same day shipping. Orders must be placed before 12PM");
                return false;
            }

            arrivalTime = new Hours(Random.Range(currentTime.Hour, 16), Random.Range(0, 59));
        }

        if (!Singleton<MoneyManager>.Instance.HasMoney(cartTotal + shippingCost))
        {
            Collective.GetManager<UIManager>().ShowMessage("Order Cancelled","Insufficient Funds");
            return false;
        }
        
        cartInventory.ToList().ForEach(item =>
        {
            var productCost = thisDist.CalculateProductSalePrice(item.FirstItemID, item.FirstItemCount);
            var newOrder = new Order(item.FirstItemID, item.FirstItemCount, productCost, arrivalDate ,arrivalTime);
            var product = ProductInfos.FirstOrDefault(productInfo => productInfo.ID == item.FirstItemID);
            if(product == null) return;
            if (!thisDist.CheckInventory(newOrder))
                Collective.GetManager<UIManager>().ShowMessage( "Order Cancelled", thisDist.Name + " does not have enough " + product.Name + " available for this order");
        });
        
        
        cartInventory.ToList().ForEach(item =>
        {
            var productCost = thisDist.CalculateProductSalePrice(item.FirstItemID, item.FirstItemCount);
            var newOrder = new Order(item.FirstItemID, item.FirstItemCount, productCost, arrivalDate ,arrivalTime);
            thisDist.AttemptPurchase(newOrder);
            Collective.Log.Info(" Placing order with " + thisDist.Name + " for " + item.FirstItemCount + " of " +
                                item.FirstItemCount + " which will arrive on " + arrivalDate + " at " +
                                arrivalTime.ToString());
        });
        
        Singleton<MoneyManager>.Instance.MoneyTransition( (cartTotal + shippingCost) * -1, MoneyManager.TransitionType.SUPPLY_COSTS);
        ShowOrderMessage(arrivalTime);
        thisDist.productInCarts.Clear();
        return true;
    }

    private void ShowOrderMessage(Hours arrivalTime)
    { 
        switch (_selectedShippingOption)
        {
            case ShippingOptions.TwoDays:
                Collective.GetManager<UIManager>().ShowMessage("Order Placed!","Your order will arrive in two days between 8AM and 4PM");
                break;
            case ShippingOptions.NextDay:
                Collective.GetManager<UIManager>().ShowMessage("Order Placed!","Your order will arrive tomorrow between 8AM and 4PM");
                break;
            case ShippingOptions.SameDay:
                Collective.GetManager<UIManager>().ShowMessage("Order Placed!","Your order will arrive before " + (arrivalTime.Hour + 1).ToString());
                break;
        }
    }

    private int CalculateArrivalDate()
    {
        var currentDay = Singleton<DayCycleManager>.Instance.CurrentDay;
        return _selectedShippingOption switch
        {
            ShippingOptions.TwoDays => currentDay + +2,
            ShippingOptions.NextDay => currentDay + 1,
            _ => currentDay
        };
    }

    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    {
        if (ProductInfos.Count == 0)
            ParseGameProducts();

        Distributors = saveData.Data.Distributors;
        if (Distributors.Count == 0)
            ActionUtility.Run<InitializeDistributors>();
    } 
    
    private void ParseGameProducts()
    { 
        var idManager = Singleton<IDManager>.Instance;
        idManager.m_Products.ForEach(product =>
        {
            var id = product.ID;
            var productName = product.ProductName;
            var price = product.BasePrice;
            var brand = product.ProductBrand;
            var quantity = product.ProductAmountOnPurchase;
            var productInfo = new ProductInfo(id, productName, brand, price,quantity);  
            ProductInfos.Add(productInfo);
        });
 
    }

    public void Tick()
    {
        for (var i = 0; i < Distributors.Count; i++)
        {
            var distributor = Distributors[i];
            distributor.ProcessInventoryUpdate();
            distributor.ProcessOrders();
        }
    }
}