using System;
using MyBox;
using UnityEngine;

namespace Collective.Systems.Entities;

public class KeyboardListener : MonoBehaviour
{ 
    private bool _skipTime = false;
    private float _lastToggleTime = 0f;
    private float _toggleCooldown = 1f; // Cooldown in seconds

    public void Stop()
    {
        Collective.Log.Info("Stopping time skip system");
        Singleton<DayCycleManager>.Instance.m_NextDayInteraction.enabled = false;
        StopTimeSkip();
        Destroy(gameObject); // Consider if you really need to destroy the gameObject
        Destroy(this);
    }
    

    private void Update()
    { 
        Singleton<DayCycleManager>.Instance.m_NextDayInteraction.enabled = true; 
        
        if(_skipTime && Math.Abs(Time.timeScale - 1) < 0.0001f) Time.timeScale = 25;

        if (EnterPressed() && Time.time - _lastToggleTime > _toggleCooldown)
        {
            if (!_skipTime)
            {
                StartTimeSkip();
            }
            else
            {
                StopTimeSkip();
            }
            _lastToggleTime = Time.time; // Update last toggle time
        }

        if (_skipTime && Input.anyKey)
        {
            return;
        }
    }

    private bool EnterPressed()
    {
        return Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return);
    }

    private void StartTimeSkip()
    {
        Collective.Log.Info("Starting time skip");
        Singleton<PlayerController>.Instance.EnableController(false, true);
        Time.timeScale = 25;
        _skipTime = true;
    }

    private void StopTimeSkip()
    {
        Collective.Log.Info("Stopping time skip");
        Time.timeScale = 1;
        Singleton<PlayerController>.Instance.EnableController(true, true);
        _skipTime = false;
    }

}