using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Systems.Actions;
using Collective.Systems.Managers;
using Collective.Utilities;
using Lean.Pool;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Collective.Components.Modals;

[Serializable]
public class Distributor
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Slug { get; set; } = "unknown";
    public DistributorType Type { get; set; } = DistributorType.SmallBusiness;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int MinLevel { get; set; } = 0;
    public int RespectLevel { get; set; } = 0;
    public PaymentTerms PaymentTerms { get; set; }
    public int JoinCost { get; set; } = 0;
    public bool IsMember { get; set; } = false;
    public string Icon { get; set; } = "";
    public ShippingCost ShippingCost { get; set; } = new(1, 2, 5);
    public List<ProductInventory> Products { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<Contract> Contracts { get; set; } = new();

    public List<ItemQuantity> productInCarts = new List<ItemQuantity>();

    public int? LastDay = null;
    public Hours? lastTime = null;
 
    
    public void ProcessInventoryUpdate()
    {
        if (Type == DistributorType.Furniture) return;
        if (Products.Count == 0) DetermineProductsToSell(); 
        if (!ShouldRun()) return; // Run once an in game day
        
        LastDay = Singleton<DayCycleManager>.Instance.CurrentDay;
        lastTime = Collective.GetNormalizedTime();
        
        // Calculate current inventory information  
        for (var i = 0; i < Products.Count; i++)
        {
            var productInventory = Products[i];
            productInventory.ProcessOrders();
            productInventory.UpdateCost(CalculateCost(productInventory.ProductId, productInventory.OrderPar));
            productInventory.UpdateUnitPrice(CalculateUnitPrice(productInventory));
            SimulateOrders(productInventory);
            ReOrderProduct(productInventory);
        }

        DetermineProductsToSell(); 
    }

    public bool CheckInventory(Order order)
    {
        var product = GetProduct(order.ProductId);
        return product != null && product.HasEnough(order.Quantity);
    }

    public bool AttemptPurchase(Order order)
    {
        var product = GetProduct(order.ProductId);
        if (product == null) return false;
        if (!product.HasEnough(order.Quantity)) return false;
        product.Sell(order.Quantity);
        Orders.Add(order);
        return true;
    }

    public float PerSaleCost(int productId)
    {
        var product = Products.FirstOrDefault(productInventory => productInventory.ProductId == productId);
        if (product == null) return 0f;
        return product.PerSalesQuantity * product.UnitPrice;
    }

    public float CalculateCartTotal(List<ItemQuantity> cartInventory)
    { 
        productInCarts = cartInventory;
        var total = 0f;
        productInCarts.ForEach(item =>
        {
            var product = Products.FirstOrDefault(productInventory => productInventory.ProductId == item.FirstItemID);
            if (product == null)
            {
                Collective.Log.Error("Could not find product " + item.FirstItemID + " within distributor " + Id);
                return;
            }

            total += (item.FirstItemCount * product.PerSalesQuantity) * product.UnitPrice; 
        });
        return total;

    }

    public float CalculateShippingCost(ShippingOptions shippingOption) => shippingOption switch
    {
        ShippingOptions.NextDay => ShippingCost.NextDay,
        ShippingOptions.TwoDays => ShippingCost.TwoDays,
        ShippingOptions.SameDay => ShippingCost.SameDay,
        _ => 0f
    };

public float CalculateProductSalePrice(int productId, int quantity)
    {
        var product = Products.FirstOrDefault(productInventory => productInventory.ProductId == productId);
        if (product == null) return 0f;
        return (quantity * product.PerSalesQuantity) * product.UnitPrice;
    }

    public void VisitWebsite()
    {
        var productViewer = Object.FindObjectOfType<ProductViewer>();
        var cart = productViewer.m_ShoppingCart;

        if (productViewer == null)
        {
            Collective.Log.Error("Could not find product viewer");
            return;
        }

        if (cart == null)
        {
            Collective.Log.Error("Could not find shopping cart");
            return;
        }

        Collective.GetManager<DistributionManager>().SetActiveWebsite(Id, productInCarts);

        // Reset the cart viewer with this distributors stuff
        productViewer.m_SalesItems?.ForEach(salesItem => Object.Destroy(salesItem.gameObject));
        productViewer.m_SalesItems?.Clear();
        cart.m_CartItems.ToList().ForEach(cartItem => cartItem.RemoveProductFromCart());
        

        Products.ForEach(productInventory =>
        {
            var salesItem = LeanPool.Spawn<SalesItem>(productViewer.m_SalesItemPrefab, productViewer.m_ProductsContent);
            salesItem.Setup(productInventory.ProductId, productViewer);
            productViewer.m_SalesItems?.Add(salesItem);
            salesItem.m_UnitPriceText.text =
                productInventory.UnitPrice.ToMoneyText(salesItem.m_TotalPriceText.fontSize);
            salesItem.m_TotalPriceText.text =
                (productInventory.UnitPrice * productInventory.PerSalesQuantity).ToMoneyText(salesItem.m_TotalPriceText
                    .fontSize);
            salesItem.transform.GetChild(1).transform.GetChild(salesItem.transform.childCount - 1).transform.GetChild(0)
                .GetComponent<TextMeshProUGUI>().text = productInventory.PerSalesQuantity.ToString();

        });
    }

    public void ProcessOrders()
    {
        if (Orders.Count == 0) return;
        var day = Singleton<DayCycleManager>.Instance.CurrentDay;
        var time = Collective.GetNormalizedTime();

        for (int i = Orders.Count - 1; i >= 0; i--)
        {
            var order = Orders[i];
            if (order.ArrivalDay != day || !time.HasPassed(order.ArrivalTime)) continue;

            ActionUtility.Run<SendOrderForDelivery, Order>(order);
            Orders.RemoveAt(i);
        }
    }
 
    private void ReOrderProduct(ProductInventory productInventory)
    {
        if (productInventory.Quantity >= productInventory.OrderPar) return;
        productInventory.CreateOrder(productInventory.OrderPar * productInventory.PerSalesQuantity);
    }
    

    // Fake Customer Orders to force product par adjustments 
    private void SimulateOrders(ProductInventory productInventory)
    {

        var orderAmount = AmountOfOrders();
        for (var i = 0; i < orderAmount; i++)
        {
            var howMany = Random.Range(1, 10);
            if (productInventory.Quantity >= howMany) continue;
            productInventory.Sell(howMany);
        }
    }

    private bool ShouldRun()
    {
        if (LastDay == null || lastTime == null) return true;
        if (LastDay == Singleton<DayCycleManager>.Instance.CurrentDay) return false; 
        
        var time = Collective.GetNormalizedTime();
        if (!time.HasPassed(lastTime)) return false;
        if (time.HoursSince(lastTime) < Random.Range(1,5)) return false; 
        return true;
    } 

    private float CalculateUnitPrice(ProductInventory productInventory)
    {
        float baseCost = productInventory.PurchaseCost;
        float markupRatio = DetermineMarkupRatio(Type);
        float newUnitPrice = baseCost * markupRatio;

        // Debugging: Log when unit price calculations occur 
  
        float finalUnitPrice = (float)Math.Round(newUnitPrice, 2); 
        return finalUnitPrice;
    }

    private float DetermineMarkupRatio(DistributorType type)
    {
        switch (type)
        {
            case DistributorType.LargeWarehouse: return Random.Range(1.15f, 1.4f);
            case DistributorType.GlobalChain: return  Random.Range(1.2f, 1.4f);
            case DistributorType.Farm: return Random.Range(1f, 1.15f);
            case DistributorType.SmallBusiness: return Random.Range(1.2f, 1.3f);
            case DistributorType.GlobalWarehouse: return  Random.Range(1f, 1.20f);
            default: return 1.20f; // Default markup
        }
    }  
    
    private int MaxProductCount() => Type switch
    {
        DistributorType.LargeWarehouse => 100,
        DistributorType.GlobalChain => 75,
        DistributorType.Farm => 25,
        DistributorType.SmallBusiness => 50,
        DistributorType.GlobalWarehouse => 200,
        _ => Random.Range(0, 10)
    }; 
    private int AmountOfOrders() => Type switch
    {
        DistributorType.LargeWarehouse => Random.Range(0,40),
        DistributorType.GlobalChain => Random.Range(0, 100),
        DistributorType.Farm => Random.Range(0, 20),
        DistributorType.SmallBusiness => Random.Range(0, 20),
        DistributorType.GlobalWarehouse => Random.Range(10, 50),
        _ => Random.Range(0, 10)
    };

    private int AmountPerPurchase(float original) => Mathf.RoundToInt(Type switch
    {
        DistributorType.Farm => original * Random.Range(1f, 1f),
        DistributorType.SmallBusiness => original * Random.Range(1f, 1f),
        DistributorType.GlobalChain => original * Random.Range(1,5),
        DistributorType.LargeWarehouse => original * Random.Range(2, 5),
        DistributorType.GlobalWarehouse => original * Random.Range(3, 10),
    });

    public ProductInventory? GetProduct(int id) => Products.FirstOrDefault(product => product.ProductId == id);

    private void DetermineProductsToSell()
    { 
        Collective.Log.Info("Distributor " + Name + " is determining products to sell");
        var entireGameProductList = Collective.GetManager<DistributionManager>().ProductInfos; 
        if(Products.Count >= MaxProductCount()) return;
        entireGameProductList.ForEach(gameProduct =>
        { 
            if(GetProduct(gameProduct.ID) != null) return;
            var newProductOrderPar = CalculateOrderPar(gameProduct.ID);
            if (newProductOrderPar == 0) return;
            var quantity = gameProduct.TypicalQuantity;

            var amountPerPurchase = AmountPerPurchase(quantity); 
            CreateProductInventory(gameProduct.ID, newProductOrderPar, amountPerPurchase);

        }); 
    }
    
    private int CalculateOrderPar(int id)
    {
        var product = Collective.GetManager<DistributionManager>().ProductInfos.Find(productInfo => productInfo.ID == id);
        if (product == null) return 0;
        switch (Type)
        {
            case DistributorType.SmallBusiness:

                if (product.HasProductType(ProductType.Alcohol))
                    return 0;
                
                if(product.HasProductType(ProductType.Vegetable, ProductType.Meat, ProductType.Dairy, ProductType.Fruit))
                    return Random.Range(2,5);

                if (product.HasProductType(ProductType.Book, ProductType.Bakery, ProductType.Condiment, ProductType.PetFood))
                    return Random.Range(1,3);
                 
                return Random.Range(0,2);
            case DistributorType.Furniture:
                return 0; 
            case DistributorType.Farm:
                if(product.HasProductType(ProductType.Meat, ProductType.Dairy))
                    return Random.Range(10,25);
                if(product.HasProductType(ProductType.Vegetable,   ProductType.Fruit))
                    return Random.Range(5,15);
                return 0;
            case DistributorType.GlobalChain:
                if (product.HasProductType(ProductType.Alcohol))
                    return Random.Range(1, 5);
                if(product.HasProductType(ProductType.Vegetable, ProductType.Meat, ProductType.Dairy, ProductType.Fruit))
                    return Random.Range(5,10);

                if (product.HasProductType(ProductType.Book, ProductType.Bakery, ProductType.Condiment,
                        ProductType.PetFood))
                    return Random.Range(1, 5);
                 
                return Random.Range(5,10);
            case DistributorType.GlobalWarehouse:
                
                if (product.HasProductType(ProductType.Alcohol))
                    return Random.Range(50,100);
                if (product.HasProductType(ProductType.Meat, ProductType.Dairy))
                    return Random.Range(15,30);

                if (product.HasProductType(ProductType.Book, ProductType.Bakery, ProductType.Condiment,
                        ProductType.PetFood))
                    return Random.Range(5,20);
                
                return Random.Range(25, 100); 
            case DistributorType.LargeWarehouse:
                
                if (product.HasProductType(ProductType.Alcohol))
                    return Random.Range(20,50);
                if (product.HasProductType(ProductType.Vegetable, ProductType.Meat, ProductType.Dairy, ProductType.Fruit))
                    return Random.Range(10, 20);

                if (product.HasProductType(ProductType.Book, ProductType.Bakery, ProductType.Condiment, ProductType.PetFood))
                    return Random.Range(5,10);
                
                return Random.Range(25, 50); 
        }

        return 0;
    }

    private void CreateProductInventory(int id, int orderPar, int amountPerPurchase)
    {
        if (GetProduct(id) != null) return;
        var inventory = new ProductInventory(id, CalculateCost(id, orderPar),orderPar * amountPerPurchase, amountPerPurchase);

        inventory.UpdateUnitPrice(CalculateUnitPrice(inventory));
        Products.Add(inventory);
        
    }

    private float CalculateCost(int id, int orderPar = 0)
    {
        // Base cost from a centralized price manager
        var baseCost = Singleton<PriceManager>.Instance.CurrentCost(id);
        
        // Calculate volume discount
        float discountFactor = 1.0f;
        if (orderPar > 50)
            discountFactor = 0.95f; // 5% discount
        else if (orderPar > 20)
            discountFactor = 0.98f; // 2% discount
        else if (orderPar > 10)
            discountFactor = 0.99f; // 1% discount

        // Apply volume discount
        var discountedCost = baseCost * discountFactor;
 

        // Calculate final cost by applying random fluctuation
        var finalCost = discountedCost;

        // Additional cost adjustments based on distributor type
        switch (Type)
        {
            case DistributorType.Farm:
                finalCost *= 1.1f;  // Markup for farm products
                break;
            case DistributorType.GlobalChain:
                finalCost *= Random.Range(.85f, 1.15f);
                break;
            case DistributorType.LargeWarehouse:
                // No additional cost adjustment for these types
                finalCost *= Random.Range(.80f, 1.15f);
                break;
            case DistributorType.Furniture:
                // Specific logic for furniture, if needed
                break;
            case DistributorType.GlobalWarehouse:
                // No additional cost adjustment for these types
                finalCost *= Random.Range(.75f, 1.10f);
                break;
            case DistributorType.SmallBusiness:
                // Small businesses might have a higher cost due to lower volume
                finalCost *= Random.Range(1.05f, 1.25f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Type), "Unknown distributor type.");
        }

        return finalCost;
    }

    private void UpdateOrderPar(int id) => GetProduct(id)?.UpdateOrderPar(CalculateOrderPar(id));

}