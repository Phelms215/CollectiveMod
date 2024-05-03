using System;
using Collective.Components.DataSets;
using MyBox;
using UnityEngine;

namespace Collective.Components.Modals;

[Serializable]
public class Order
{
    public readonly int ProductId;
    public readonly int Quantity;
    public int BoxCount = 1;
    public readonly float Cost;
    public readonly int OrderDay;
    public readonly Hours OrderTime;
    public readonly int ArrivalDay;
    public readonly Hours ArrivalTime;

    public Order(int productId, int quantity, float cost, int arrivalDay, Hours arrivalTime)
    {
        ProductId = productId;
        Quantity = quantity;
        Cost = cost;
        ArrivalDay = arrivalDay;
        ArrivalTime = arrivalTime;
        OrderDay = Singleton<DayCycleManager>.Instance.CurrentDay;
        OrderTime = Collective.GetNormalizedTime();
    }

    public void UpdateBoxCount(int boxCount) => BoxCount = boxCount;
    
    
}