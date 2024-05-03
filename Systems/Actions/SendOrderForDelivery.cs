using System.Linq;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Managers; 
using MyBox;
using UnityEngine;

namespace Collective.Systems.Actions;

// Function to handle automatic opening and closing of the store hours
public class SendOrderForDelivery : IGameAction<Order>
{
    public void Execute(Order order)
    {
        var num = 0;
        var deliveryManager = Singleton<DeliveryManager>.Instance;
        var deliveryPosition = deliveryManager.m_DeliveryPosition;
        Collective.Log.Info("Sending order for product id " + order.ProductId + " with quantity " + order.Quantity +" and box count " + order.BoxCount);
        for (int index = 0; index < order.Quantity; ++index)
        {
            for (int index2 = 0; index2 < order.BoxCount; ++index2)
                Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(order.ProductId),
                    deliveryPosition.position + Vector3.up * deliveryManager.space * (float)num, Quaternion.identity,
                    deliveryManager.transform).Setup(order.ProductId, true);
        }
    }
    
}