using System.Collections;
using Collective.Components.DataSets;
using Collective.Components.Interfaces;
using Collective.Systems.Entities;
using Collective.Systems.Managers;
using MyBox;
using UnityEngine;

namespace Collective.Systems.Actions;

public class CheckStoreState : IGameAction
{  
    public void Execute()
    {
        var permitManager = Collective.GetManager<PermitManager>();
        var gameDataManager = Collective.GetManager<GameDataManager>(); 
        var isOpen = Singleton<StoreStatus>.Instance.IsOpen;
        
        

        if (isOpen && !permitManager.ValidateCurrentHour())
        {
            Collective.Log.Info("Auto closing store due to lack of permit");
            CloseStore();
        }

        var settings = gameDataManager.GetSaveData().Settings;
        var storeHours = settings.StoreHours;
        var currentTime = Collective.GetNormalizedTime();

        if (settings.AutoOpen)
        {
            if (currentTime.Equals(storeHours.Open) && !isOpen) OpenStore();
            if (currentTime.Equals(storeHours.Close) && isOpen) CloseStore();
        } 
 
         
        if (storeHours.OutsideHours(currentTime) && !isOpen)  
            Collective.StartTimeSkip();
        
    } 
 
    private void CloseStore()
    {
        if (!Singleton<StoreStatus>.Instance.IsOpen) return;
        Collective.Log.Info("CheckStoreState: Closing store");
        Singleton<StoreStatus>.Instance.IsOpen = false;
    }

    private void OpenStore()
    {
        if (Singleton<StoreStatus>.Instance.IsOpen) return;
        Collective.Log.Info("CheckStoreState: Opening store");
        Singleton<StoreStatus>.Instance.IsOpen = true;
    }
}
