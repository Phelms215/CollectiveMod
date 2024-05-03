using System;
using System.Collections.Generic;
using Collective.Components.DataSets;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Collective.Components.Modals;


[Serializable]
public class ProductInventory
{
    [SerializeField] public int ProductId { get; set; }
    [SerializeField] public float PurchaseCost { get; set; }
    [SerializeField] public float UnitPrice { get; set; }
    [SerializeField] public int PerSalesQuantity { get; set; }
    [SerializeField] public int Quantity { get; set; } = 0;
    [SerializeField] public List<Order> PendingOrders { get; set; } = new();
    [SerializeField] public float TotalInvestment { get; set; } = 0f;
    [SerializeField] public float TotalProfit { get; set; } = 0f;
    [SerializeField] public int OrderPar { get; set; } = 0;

    public ProductInventory()
    {
        
    }
    
    public ProductInventory(int productId, float cost, int orderPar, int perSalesQuantity)
    {
        ProductId = productId;
        PurchaseCost = (float)Math.Round(cost, 2);
        Quantity = orderPar;
        OrderPar = orderPar;
        PerSalesQuantity = perSalesQuantity; 
    }

    public void ProcessOrders()
    {
        if (PendingOrders.Count == 0) return;
        var day = Singleton<DayCycleManager>.Instance.CurrentDay;
        var time = Collective.GetNormalizedTime();

        for (var i = 0; i < PendingOrders.Count; i++)
        {
            var order = PendingOrders[i];
            if (order.ArrivalDay != day || !order.ArrivalTime.HasPassed(time))
                continue;

            AddQuantity(order.Quantity);
            TotalInvestment += order.Cost;
            PendingOrders.Remove(order);
        }
    }

    public bool OutOfStock() => Quantity == 0;
    public bool HasEnough(int quantity) => Quantity >= quantity;
    private void RemoveQuantity(int quantity) => Quantity -= quantity;
    private void AddQuantity(int quantity) => Quantity += quantity;
    public void UpdateCost(float cost) => PurchaseCost =  (float)Math.Round(cost, 2); 
    public void UpdateUnitPrice(float cost) => UnitPrice = (float)Math.Round(cost, 2);  
    
    public void UpdateOrderPar(int orderPar) => OrderPar = orderPar;


    public void CreateOrder(int quantity)
    {
        var time = new Hours(Random.Range(10, 15), 0);
        var day = Singleton<DayCycleManager>.Instance.CurrentDay;
        PendingOrders.Add(new Order(ProductId, quantity, PurchaseCost, day + 2, time));
    }
     
    
    public void Sell(int quantity)
    { 
        RemoveQuantity(quantity);
        TotalProfit += (PerSalesQuantity * quantity) * PurchaseCost;
    }

    

    

}