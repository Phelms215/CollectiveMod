using System.Collections;
using Collective.Components.Interfaces;
using Collective.Systems.Managers; 
using MyBox;
using UnityEngine;

namespace Collective.Systems.Actions;

// Function to handle automatic opening and closing of the store hours
public class ReloadStore : IGameAction
{
    public void Execute()
    {
        var gameDataManager = Collective.GetManager<GameDataManager>();
        if (gameDataManager.StoreOpen())
            OpenStore();
        else
            CloseStore();

        // Track the light system
        Singleton<StoreLightManager>.Instance.TurnOn = gameDataManager.LightsOn(); 
        
        // TP Player to where they were 
        var player = GameObject.Find("Player").transform;
        if (Collective.GetManager<GameDataManager>().GetPlayerPosition() == Vector3.zero) return;
        Singleton<PlayerController>.Instance.transform.SetPositionAndRotation(gameDataManager.GetPlayerPosition(),
            gameDataManager.GetPlayerRotation());
        
        var time = Collective.GetManager<GameDataManager>().GetTime();
        Collective.Log.Info($"Restoring Time to {time.ToString()}");
        var hour = time.Hour;
        var am = true;
        if (hour >= 13) 
            hour -= 12; 
        
        if (hour == 0) 
            hour = 12;  
        
        if(time.Hour >= 12) 
            am = false;
        
            
        
        var dayCycleManager = Singleton<DayCycleManager>.Instance;
        dayCycleManager.m_DayStartingTime = 0;
        dayCycleManager.m_DayCycling = false;
        dayCycleManager.m_DayDurationInRealtime = 20;
        dayCycleManager.m_DayDurationInGameTimeInSeconds = 86400f;
        dayCycleManager.m_DayDurationInReelTimeInSeconds = dayCycleManager.m_DayDurationInRealtime * 60f;
        dayCycleManager.m_GameTimeScale = dayCycleManager.m_DayDurationInGameTimeInSeconds / dayCycleManager.m_DayDurationInReelTimeInSeconds;

        dayCycleManager.CurrentTime = time.Hour;
        dayCycleManager.m_CurrentTimeInFloat = time.Hour;
        dayCycleManager.m_CurrentTimeInHours = hour;
        dayCycleManager.m_AM = am;
        dayCycleManager.m_CurrentTimeInMinutes = time.Minute; 
        dayCycleManager.m_DayCycling = true;
        dayCycleManager.UpdateLighting();
        
        dayCycleManager.StartCoroutine(ModifiedDayCycle()); 
    } 


    
    private void CloseStore()
    { 
        if (!Singleton<StoreStatus>.Instance.IsOpen) return;
        Singleton<StoreStatus>.Instance.IsOpen = false;
    }

    private void OpenStore()
    {
        if (Singleton<StoreStatus>.Instance.IsOpen) return; 
        Singleton<StoreStatus>.Instance.IsOpen = true;
    }

    private static void FinishDay()
    { 
        var dayCycleManager = Singleton<DayCycleManager>.Instance;
        Collective.Log.Info("Finishing day");
        ++dayCycleManager.CurrentDay; 
        dayCycleManager.CurrentTime = 0.0f;
        dayCycleManager.m_CurrentTimeInFloat = 0.0f;
        dayCycleManager.m_DayPercentage = 0.0f;
        dayCycleManager.m_CurrentTimeInHours = 0;
        dayCycleManager.m_CurrentTimeInMinutes = 0;
        dayCycleManager.m_AM = true;
        
    }

    private static IEnumerator ModifiedDayCycle()
    {
        var dayCycleManager = Singleton<DayCycleManager>.Instance;
        
        dayCycleManager.UpdateGameTime();
        dayCycleManager.m_DayPercentage =
            dayCycleManager.CurrentTime / dayCycleManager.m_DayDurationInGameTimeInSeconds;
        
        dayCycleManager.UpdateLighting();
        dayCycleManager.LensFlare = dayCycleManager.m_DayPercentage <= (double)dayCycleManager.m_DisablingLensFlarePercentage;

        // Continuously update the time within the loop
        while (true) // Use true to keep the loop running until we explicitly break out
        {
            var currentTime = Collective.GetNormalizedTime();
            if (currentTime is { Hour: 23, Minute: 59 }) 
                FinishDay(); 

            dayCycleManager.UpdateTimer();
            dayCycleManager.UpdateGameTime();
            dayCycleManager.UpdateLighting();

            yield return null; // Wait for the next frame before continuing
        }
    }

}