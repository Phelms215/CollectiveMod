using System.Collections.Generic;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Systems.Actions;

public class GenerateRestockerTasks : IGameAction
{ 

    private float m_MinFillRateForDisplaySlotsToRestock = 0.75f;
    
   
    private StoreInventory _storeInventory = new ();


    public void Execute()
    {
        // Find Boxes To Put On Storage Shelf
        SearchForUnassignedBoxes();

        // Inventory Storage Racks
        SearchStorageRacks();

        // Find Display Shelves who need items
        SearchDisplayShelves();
        Collective.GetManager<DistributionManager>().UpdateInventory(_storeInventory);
        Collective.GetManager<StaffManager>().UpdateRestockerTasks(_storeInventory.GetRestockerTasks());

    }

    private void SearchForUnassignedBoxes()
    {
        // For deliveries 
        
    }
    
    private void SearchStorageRacks()
    {
        Singleton<RackManager>.Instance.m_RackSlots.Values.ForEach(list => list.ForEach(rackSlot =>
        {
            if (!rackSlot.HasProduct)
            {
                _storeInventory.AddEmptyRackSlot(rackSlot);
                return;
            }
            
            var productID = rackSlot.Data.ProductID;
            _storeInventory.AddRackSlot(productID, rackSlot, rackSlot.Full);
            
        }));
    }
    
    private void SearchDisplayShelves()
    {
        var productDisplays = Singleton<DisplayManager>.Instance.m_DisplayedProducts;
        productDisplays.ForEach(x =>
        {
            var productId = x.Key;
            var displayShelfList = x.Value; 

            // Loop through all display shelves that have this product
            displayShelfList.ForEach(displaySlot =>
            {
                // No data to check
                if (displaySlot.Data == null || displaySlot.Data.FirstItemID <= 0) return;
                _storeInventory.AddDisplaySlot(productId, displaySlot, displaySlot.Full, displaySlot.Data.FirstItemCount);
            });

            
        });
    }


    
}