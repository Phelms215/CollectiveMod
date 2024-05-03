using System;
using System.Collections.Generic;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces; 
using Collective.Systems.UI;
using Collective.Systems.UI.ComputerTabs;
using Collective.Utilities; 
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Collective.Systems.Managers;

public class UIManager : ManagerUtility, IManage, ITriggerOnSceneLoad 
{
    private readonly Dictionary<Views, IView> _viewData = new();

    private Overlay? _overlay; 
    
    public void OnSceneLoaded()
    {
        _viewData.Clear();
        LoadTab<SettingsTab>(Views.Settings);
        LoadTab<HiringTab>(Views.Hiring);
        LoadTab<DistributorTab>(Views.Distributor);
        LoadTab<LicenseTab>(Views.License);
        ManagementAppListeners();
    }

    public void RefreshTab(Views activeTab)
    { 
        if (!_viewData.ContainsKey(activeTab)) return;
        _viewData[activeTab].UpdateView();
    }


    public void ShowMessage(string title, string message)
    {
        if (_overlay != null) return;

        if (Object.FindObjectOfType<Overlay>() != null)
        {
            Collective.Log.Info("Overlay already exists, not showing dialog");
            return;
        }

        _overlay = new GameObject(PluginInfo.PLUGIN_NAME + "Overlay").AddComponent<Overlay>();
        _overlay.ShowMessage(title, message);
        _overlay.Show();
    }
     
    public void Confirmation(string title, string message, Func<bool>? cancelAction = null,
        Func<bool>? submitAction = null)
    { 
        if (_overlay != null) return;
        if (Object.FindObjectOfType<Overlay>() != null)
        {
            Collective.Log.Info("Overlay already exists, not showing dialog");
            return;
        }

        _overlay = new GameObject(PluginInfo.PLUGIN_NAME + "Overlay").AddComponent<Overlay>();
        _overlay.Confirmation(title, message, cancelAction, submitAction);
        _overlay.Show();
    }


    public void LoadForm<TForm, TFormArg>(string formName, string formTitle, TFormArg arg)
        where TForm : IOverlayForm<TFormArg>, new()
        where TFormArg : class
    { 
        if (_overlay != null) return;
        if (Object.FindObjectOfType<Overlay>() != null)
        {
            Collective.Log.Info("Overlay already exists, not showing dialog");
            return;
        }

        _overlay = new GameObject(PluginInfo.PLUGIN_NAME + "Overlay").AddComponent<Overlay>();
        _overlay.LoadForm<TForm, TFormArg>(formName, formTitle, arg);
    }

    public void ShowToast(string message)
    {
        Collective.Log.Info("Should display a toast message.. " + message); 
    }


    public void HideOverlay()
    {
        if(_overlay == null) return;
        _overlay.Hide();
        Object.Destroy(_overlay.gameObject);
        Object.Destroy(_overlay);
        _overlay = null; 
    } 
    

    public void DisplayMessage(string message)
    {
        LoadTab<InGameMessage>(Views.InGameMessage);
        var thisTab = GetTab<InGameMessage>(Views.InGameMessage);
        if (thisTab == null) return;
        thisTab.DisplayMessage(message);
        DestroyTab(Views.InGameMessage, 5); 
    }


    private void LoadTab<T>(Views view) where T : Component, IView
    {
        if (Object.FindObjectOfType<T>() != null) return;
        if (_viewData.ContainsKey(view))
        {
            Collective.Log.Info($"View {view} already exists, not loading");
            return;
        }
        
        _viewData.Add(view, new GameObject(PluginInfo.PLUGIN_NAME + "-" + view).AddComponent<T>());
    }

    private void DestroyTab(Views view, float delay = 0)
    {
        if (!_viewData.ContainsKey(view)) return;
        _viewData[view].Kill(delay);
        _viewData.Remove(view); 
    }

    private void HidePanels()
    {
        _viewData[Views.Settings].Hide();
        _viewData[Views.Hiring].Hide();
        _viewData[Views.License].Hide(); 
    }

    
    
    private T? GetTab<T>(Views view) where T : Component, IView
    {
        if (!_viewData.ContainsKey(view)) return null;
        return _viewData[view] as T;
    }
 
    private void ManagementAppListeners()
    {
        var buttonParent = GameObject.Find("---GAME---/Computer/Screen/Management App/Taskbar/Buttons/");
        var buttonList = buttonParent.GetComponentsInChildren<Button>();
        foreach (var button in buttonList)
        {
            switch (button.name)
            {
                case "Licenses Tab Button":
                    button.onClick.AddListener(() =>
                    {
                        HidePanels();
                        _viewData[Views.License].Show(); 
                    });
                    break;
                case "Hiring Tab Button":
                    button.onClick.AddListener(() =>
                    {
                        HidePanels();
                        _viewData[Views.Hiring].Show(); 
                    });
                    break;
                case "Bank Tab Button":
                    button.onClick.AddListener(() =>
                    {
                        HidePanels();
                        _viewData[Views.Settings].Show(); 
                    });
                    break;
                default:
                    button.onClick.AddListener(HidePanels);
                    break;
            }
        }
    }

    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    {
         
    }
}