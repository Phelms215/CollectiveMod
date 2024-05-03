using System;
using System.Collections.Concurrent;
using Collective.Components.DataSets;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Utilities;
using MyBox;

namespace Collective.Systems.Managers;

public class EconomyManager : ManagerUtility, IManage
{
    public EconomyManager() : base()
    {
        
    }

    public void LoadSaveData(SaveData saveData)
    {  
        
    }

    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    { 
    } 
}