using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Actions;
using Collective.Utilities;
using JetBrains.Annotations;
using MyBox;

namespace Collective.Systems.Managers;

public class PermitManager : ManagerUtility, IManage
{
    private readonly List<Permit> _registeredPermits = new();
    private List<string> _unlockedPermits = new();
    public List<string> GetUnlockedPermits() => _unlockedPermits;

    public List<Permit> GetPermits(PermitType? type) =>
        type == null ? _registeredPermits : _registeredPermits.FindAll(permit => permit.Type == type);

    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    {
        // Generate Permits 
        if(_registeredPermits.Count == 0)
            ActionUtility.Run<InitializePermits>();
        
        _unlockedPermits = saveData.Data.GetUnlockedPermits();
    }
    

    public void RegisterPermit(Permit permit) => _registeredPermits.Add(permit);

    public void UnlockPermit(string permitId)
    {
        if (_unlockedPermits.Contains(permitId)) return; 
        var cost = _registeredPermits.Find(permit => permit.ID == permitId).Cost * -1;
        Singleton<MoneyManager>.Instance.MoneyTransition(cost, MoneyManager.TransitionType.UPGRADE_COSTS);
        _unlockedPermits.Add(permitId);
    }

    public bool CanBuyProduct(ProductInfo productID)
    {
        if(productID.HasProductType(ProductType.Dairy) &&!IsUnlocked("DairyProducts")) return false;
        if(productID.HasProductType(ProductType.Meat) &&!IsUnlocked("MeatsSeafood")) return false;
        if(productID.HasProductType(ProductType.Fish) &&!IsUnlocked("MeatsSeafood")) return false;
        if(productID.HasProductType(ProductType.Alcohol) &&!IsUnlocked("BeersLiquor")) return false;
        return true;
    }

    public bool ValidateCurrentHour()
    { 
        if (IsUnlocked("247Hours")) return true; // 24/7 hours permit bypasses all other checks
        var currentHours = Collective.GetNormalizedTime();
        var currentTotalMinutes = currentHours.Hour * 60 + currentHours.Minute;
 
        // Early Bird: Can open as early as 4:00
        if (IsUnlocked("EarlyBird") && currentTotalMinutes >= 4 * 60) return true;

        // Extend Hours: Can close as late as 21:00
        if (IsUnlocked("ExtendHours") && currentTotalMinutes <= 21 * 60) return true;

        // Late Night Store: Can close as late as 23:00
        if (IsUnlocked("LateNightStore") && currentTotalMinutes <= 23 * 60) return true;
 
        // General validation
        // Ensure that the current time is not before the default opening hour
        if (currentTotalMinutes < Key.DefaultStoreOpenHour * 60) {
            Collective.Log.Info($"Attempt to open too early at {currentHours.Hour}:{currentHours.Minute.ToString("00")}. Store cannot open before {Key.DefaultStoreOpenHour}:00.");
            return false;
        }

        // Ensure that the current time is not after the default closing hour
        if (currentTotalMinutes > Key.DefaultStoreCloseHour * 60) {
            Collective.Log.Info($"Attempt to open too late at {currentHours.Hour}:{currentHours.Minute.ToString("00")}. Store must close by {Key.DefaultStoreCloseHour}:00.");
            return false;
        }

        return true;
    }
    public bool ValidateStoreHours(StoreHours storeHours)
    {    
        if (IsUnlocked("247Hours")) return true; // 24/7 hours permit bypasses all other checks

        // Combining hour and minute into total minutes for easier comparison
        var openingTotalMinutes = storeHours.Open.Hour * 60 + storeHours.Open.Minute;
        var closingTotalMinutes = storeHours.Close.Hour * 60 + storeHours.Close.Minute;

        // Basic sanity check: store must open before it closes
        if (openingTotalMinutes >= closingTotalMinutes) return false;

        // Early Bird Permit: Store can open as early as 4:00 AM
        bool earlyBirdCondition = IsUnlocked("EarlyBird") ? openingTotalMinutes >= 240 : openingTotalMinutes >= Key.DefaultStoreOpenHour * 60;
    
        // Extend Hours Permit: Store can close as late as 21:00
        bool extendHoursCondition = IsUnlocked("ExtendHours") ? closingTotalMinutes <= 1260 : closingTotalMinutes <= Key.DefaultStoreCloseHour * 60;

        // Late Night Store Permit: Store can close as late as 23:00
        bool lateNightCondition = IsUnlocked("LateNightStore") ? closingTotalMinutes <= 1380 : closingTotalMinutes <= Key.DefaultStoreCloseHour * 60;

        // Must satisfy all applicable conditions
        return earlyBirdCondition && extendHoursCondition && lateNightCondition;
    } 

    public StoreHours? OperatingRestrictions()
    {
        var openHour = 8;
        var closeHour = 18;
        if (IsUnlocked("247Hours")) return null;
        if (IsUnlocked("EarlyBird")) openHour = 4;
        if (IsUnlocked("ExtendHours")) closeHour = 21;
        if (IsUnlocked("LateNightStore")) closeHour = 23;
        return new StoreHours(openHour, 0, closeHour, 0);
    }

    public bool IsUnlocked(string id) => _unlockedPermits.Contains(id);

    private Permit GetPermit(string id) => _registeredPermits.Find(permits => permits.ID == id);
    private bool PermitExists(string id) => _registeredPermits.Count(permits => permits.ID == id) > 0;

}