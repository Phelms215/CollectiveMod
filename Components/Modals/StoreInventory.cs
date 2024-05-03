using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using MyBox;

namespace Collective.Components.Modals;

public class StoreInventory
{
    private List<RackSlot> _emptyRackSlots = new List<RackSlot>();

    public List<InventoryItem> InventoryItems { get; } = new();
    
    public List<RackSlot> GetRackSlotsWithSpace(int productID) => _emptyRackSlots.FindAll(i => i.Data.ProductID == productID && !i.Full).ToList();


    public void AddInventoryItem(InventoryItem inventoryItem)
    {
        if(InventoryItems.Contains(inventoryItem))
            InventoryItems.Remove(inventoryItem);
        
        InventoryItems.Add(inventoryItem);
    }
    
    public RackSlot? GetEmptyRackSlot() => _emptyRackSlots.FirstOrDefault();

    public void AddEmptyRackSlot(RackSlot rackSlot)
    {
        var getBySlot = GetItemByRackSlot(rackSlot);
        if (getBySlot != null)
            InventoryItems.Remove(getBySlot);

        if (_emptyRackSlots.Contains(rackSlot))
            _emptyRackSlots.Remove(rackSlot);

        _emptyRackSlots.Add(rackSlot);
    }

    public InventoryItem? GetItemByDisplaySlot(DisplaySlot displaySlot) => InventoryItems.FirstOrDefault(i => i.DisplaySlot == displaySlot);
    
    public InventoryItem? GetItemByRackSlot(RackSlot rackSlot) =>  InventoryItems.FirstOrDefault(i => i.RackSlot == rackSlot);

    public List<InventoryItem> GetItemsByProductID(int productID) => InventoryItems.FindAll(i => i.ProductID == productID);
    
    public List<InventoryItem> FindItemInventory(int productID) => InventoryItems.FindAll(i => i.ProductID == productID && i.RackSlot != null);

    public void AddDisplaySlot(int productID, DisplaySlot displaySlot, bool isFull, int quantity)
    {
        var getDisplaySlot = GetItemByDisplaySlot(displaySlot);
        if (getDisplaySlot != null)
            InventoryItems.Remove(getDisplaySlot);
        
        InventoryItems.Add(new InventoryItem
        {
            ProductID = productID,
            DisplaySlot = displaySlot,
            ShouldRestock = IsDisplaySlotAvailableToRestock(displaySlot),
            IsFull = isFull
        });
    }

    public void AddRackSlot(int productID, RackSlot rackSlot, bool isFull)
    {
        var getRackSlot = GetItemByRackSlot(rackSlot);
        if (getRackSlot != null)
            InventoryItems.Remove(getRackSlot);
        
        if (_emptyRackSlots.Contains(rackSlot))
            _emptyRackSlots.Remove(rackSlot);
        
        InventoryItems.Add(new InventoryItem
        {
            ProductID = productID,
            RackSlot = rackSlot,
            IsFull = isFull
        });
    }

    public List<int> GetAllProductIDs() => InventoryItems.Select(i => i.ProductID).Distinct().ToList();



    public class InventoryItem
    {
        public int ProductID { get; set; }
        public DisplaySlot? DisplaySlot { get; set; }
        public RackSlot? RackSlot { get; set; }
        public bool IsFull { get; set; }
        public bool ShouldRestock { get; set; }
    }


    private bool IsDisplaySlotAvailableToRestock(DisplaySlot displaySlot)
    {
        if (!displaySlot.HasProduct) return false;
        var productSo = Singleton<IDManager>.Instance.ProductSO(displaySlot.Data.FirstItemID); 
        return displaySlot.ProductCount < productSo.GridLayoutInStorage.productCount;
    } 

    public List<RestockerTask> GetRestockerTasks()
    {
        List<RestockerTask> restockerTasks = new List<RestockerTask>();
        InventoryItems.FindAll(i => i.DisplaySlot != null && i.ShouldRestock).ForEach(displaySlotData =>
        {
            if (restockerTasks.Any(t => t.TargetDisplaySlot == displaySlotData.DisplaySlot)) ;
            var findRack = FindItemInventory(displaySlotData.ProductID).FirstOrDefault();
            if (findRack?.RackSlot == null || displaySlotData?.DisplaySlot == null) return;
            restockerTasks.Add(new RestockerTask(displaySlotData.ProductID, displaySlotData.DisplaySlot));

        });
        return restockerTasks;
    }

}

