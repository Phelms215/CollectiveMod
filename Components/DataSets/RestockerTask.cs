using System;
using System.Threading.Tasks;
using Collective.Components.Definitions;
using Collective.Components.Modals;
using UnityEngine;

namespace Collective.Components.DataSets;

public class RestockerTask
{
    public readonly Guid ID = Guid.NewGuid();
    public int ProductId { get; set; }
    public DisplaySlot? TargetDisplaySlot { get; set; }
    public RackSlot? TargetRackSlot { get; set; }
    public Vector3 TargetPosition { get; set; }
    public RestockerTaskTypes TaskType { get; set; }
    
    public bool HasStarted { get; set; }
    public bool IsAssigned { get; set; }
    
    public Employee AssignedEmployee { get; set; }
    
    

    public RestockerTask(int productId, DisplaySlot targetDisplaySlot)
    {
        ProductId = productId;
        TargetDisplaySlot = targetDisplaySlot;
        TaskType = RestockerTaskTypes.RestockShelf;
    }
 


}