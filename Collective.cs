using System; 
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Patches;
using Collective.Systems.Actions;
using Collective.Systems.Entities;
using Collective.Systems.Managers;
using Collective.Utilities;
using MyBox;
using UnityEngine.SceneManagement;

namespace Collective;

using BepInEx;  
using UnityEngine;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Collective : BaseUnityPlugin
{
    private readonly LogUtility _logUtility;
    private static Collective _instance = new();
    private readonly List<IManage> _managerData = new();

    private KeyboardListener? _keyboardListener;

    private const int TickRate = 1;
    private float timeSinceLastUpdate = 0.0f; 

    private GameObject? _uiGameObject = null;
    private bool _isInitialized = false;
    
    public static bool IsInitialized => _instance._isInitialized;
    public static LogUtility Log => _instance._logUtility;

    public Collective()
    {
        _logUtility = new LogUtility(Logger);
    } 
     

    public static T GetManager<T>() where T : IManage
    {
        var response = _instance._managerData.OfType<T>().FirstOrDefault();
        if (response != null) return response;
        Collective.Log.Error($"Could not find manager of type {typeof(T).Name}");
        throw new NullReferenceException();

    }

    public static void StartTimeSkip()
    {
        if (_instance._keyboardListener != null) return;
        _instance._keyboardListener = new GameObject("Collective-TimeSkip").AddComponent<KeyboardListener>();
    }
 
    
    public static Hours GetNormalizedTime()
    {
        
        // Combining hour and minute into total minutes for easier comparison
        var dayManager = Singleton<DayCycleManager>.Instance;
        var thisHour = dayManager.CurrentHour; 
        if (dayManager.m_AM)
        {
            if (thisHour == 24) thisHour = 0; // 24 adjustment
        }
        else
        {
            if (thisHour != 12) thisHour += 12; // if its 1pm or later add 12 to convert to 24 hour time
        }
 
        return new Hours(thisHour, dayManager.CurrentMinute); 
    }
    
    private void Awake()
    {
        LoadAndOrganizeManagers(); 
        _instance = this; 
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
 

    private void LoadAndOrganizeManagers()
    { 
        _managerData.Add(new GameDataManager());
        _managerData.Add(new PermitManager());
        _managerData.Add(new DistributionManager());
        _managerData.Add(new EconomyManager());
        _managerData.Add(new StaffManager());
        _managerData.Add(new UIManager());
    } 
    
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main Scene") return;
        _instance.CreatePause();
        _instance.EnsurePatchUtilityExists(); 
        
        
        foreach (var manager in _instance._managerData)
            if (manager is ITriggerOnSceneLoad savable)
                savable.OnSceneLoaded();
        
        Collective.Log.Info("Disable Tutorial & Mission system");
         GameObject.FindObjectOfType<MissionSystem>().gameObject.SetActive(false);
         GameObject.FindObjectOfType<OnboardingManager>().SkipOnboarding = true;
         
          
        

        Collective.Log.Info("Register Events");
        var daymanager = Singleton<DayCycleManager>.Instance;
        daymanager.OnTimeChanged -= _instance.OnTimeChanged; // Unsubscribe to avoid duplicate subscriptions
        daymanager.OnTimeChanged += _instance.OnTimeChanged;  
        
        Collective.Log.Info("Reloading existing settings");
        ActionUtility.Run<ReloadStore>();  
        if(_instance._uiGameObject != null) 
            _instance._uiGameObject.SetActive(true);
        
        Singleton<StoreStatus>.Instance.onStoreStatusChaned += (result) =>
            Collective.GetManager<GameDataManager>().SetStoreStatus(Singleton<StoreStatus>.Instance.IsOpen);

        // First day set to 12pm 
        if(GetNormalizedTime().Hour == 12 && Singleton<DayCycleManager>.Instance.CurrentDay == 1) 
            Singleton<DayCycleManager>.Instance.m_AM = false; 
        
        Collective.Log.Info("Unfreeze Time");
        Time.timeScale = 1f; 
        _instance._isInitialized = true;
         
    } 
    private void CreatePause()
    {
        Time.timeScale = 0;
        _uiGameObject = GameObject.Find("---UI---"); 
        _uiGameObject.SetActive(false);
    }
 
    private void EnsurePatchUtilityExists()
    {
        if (FindObjectOfType<PatchUtility>() == null)
            new GameObject(PluginInfo.PLUGIN_NAME + "HarmonyPatcher").AddComponent<PatchUtility>();
    }


    private void OnTimeChanged()
    {
        if (!_instance._isInitialized) return;
        ActionUtility.Run<CheckStoreState>();
        try
        {
            foreach (var manager in _instance._managerData)
                if (manager is ITick tickable)
                    tickable.Tick();

            if (_instance._keyboardListener == null) return;
            _instance.HideTimeSkip();
        }
        catch (Exception exception)
        {
            Collective.Log.Error("Error in OnTimeChanged");
            Collective.Log.Error(exception.Message);
            Collective.Log.Error(exception.StackTrace);
        }
    }


    private void HideTimeSkip()
    {
        if(_keyboardListener == null) return;
        var storeHours = Collective.GetManager<GameDataManager>().GetSaveData().Settings.StoreHours;
        var targetTime = new Hours(storeHours.Open.Hour - 1, storeHours.Open.Minute);
        var currentTime = Collective.GetNormalizedTime();
        if (targetTime.Equals(currentTime))
        { 
            if (_instance._keyboardListener == null) return;
            _instance._keyboardListener.Stop();
            _instance._keyboardListener = null;
        }
    }

    
    private void OnDestroy()
    {
        EventUtility.ClearAllSubscriptions();
    }

}