using System;
using UnityEngine;
using System.IO;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Actions;
using Collective.Utilities;
using MyBox;
using Newtonsoft.Json;

namespace Collective.Systems.Managers;

public class GameDataManager : ManagerUtility, IManage, ITriggerOnSceneLoad
{
     
    private SaveData _saveData = new();
    private readonly string _saveFolder = Application.persistentDataPath;
    public SaveData GetSaveData() => _saveData;
    public void OnSceneLoaded() => Load(Singleton<SaveManager>.Instance.m_CurrentSaveFilePath);

    public void Save(string saveFileName) =>
        CreateSaveFile(Path.Combine(_saveFolder, saveFileName.Replace(".es3", Key.SaveFileExtension)));

    public Vector3 GetPlayerPosition() => _saveData.GetPlayerPosition();
    public Quaternion GetPlayerRotation() => _saveData.GetPlayerRotation();
 

    public void UpdateAutoOpenClose(bool isChecked)
    {
        _saveData.SetAutoOpen(isChecked);
        EventUtility.Invoke<SaveData.MiscSettings>(ModEventType.SettingsUpdated, _saveData.Settings);
    }

    public bool GetAutoOpenSetting() => _saveData.Settings.AutoOpen;
    public bool GetApplicantSearchStatus() => _saveData.Settings.SearchForApplicants;

    public void UpdateStoreHours(StoreHours hours)
    {
        _saveData.SetStoreHours(hours);
        EventUtility.Invoke<SaveData.MiscSettings>(ModEventType.SettingsUpdated, _saveData.Settings);
    }
    
    public void SetAcceptingApplicants(bool accepting)
    {
        _saveData.SetSearchForApplicants(accepting);
        EventUtility.Invoke<SaveData.MiscSettings>(ModEventType.SettingsUpdated, _saveData.Settings);

    }

    public void SetStoreStatus(bool isOpen) => _saveData.SetStoreStatus(isOpen); 
    
    public Hours GetTime() => _saveData.Settings.CurrentTime;
    
    public bool LightsOn() => _saveData.Settings.LightOn;
    
    public bool StoreOpen() => _saveData.Settings.StoreOpen;
    
    //
    // Internal Functions 
    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    { 

    }
    
    private void UpdateTime()
    { 
        var time = Collective.GetNormalizedTime();
        _saveData.UpdateTime(time.Hour, time.Minute);
    } 
    
    
    //
    // Core Save Functions
    //
       
    private void CreateSaveFile(string fullPath)
    {
        RecordGameData();
        var saveData = JsonConvert.SerializeObject(_saveData, Formatting.None);
        File.WriteAllText(fullPath, saveData);
    }

    private void RecordGameData()
    {
        _saveData.Distributors = Collective.GetManager<DistributionManager>().Distributors.ToList();
        Collective.GetManager<StaffManager>().RecordEmployeePosition();
        _saveData.Employees = Collective.GetManager<StaffManager>().Employees.ToList();
        _saveData.Applicants = Collective.GetManager<StaffManager>().Applicants.ToList();
        _saveData.UpdatePermits(Collective.GetManager<PermitManager>().GetUnlockedPermits().ToList());
        
        UpdateTime();
        _saveData.Settings.LightOn = Singleton<StoreLightManager>.Instance.TurnOn;
        _saveData.UpdatePlayerPosition();
    }

    private void Load(string saveFileName)
    {

        var newFilePath = Path.Combine(_saveFolder, saveFileName.Replace(".es3", Key.SaveFileExtension));
        if (File.Exists(newFilePath))
            LoadFromFile(newFilePath);
        else
            CreateSaveFile(newFilePath);
        
        EventUtility.Invoke<SaveData>(ModEventType.InitialLoad, _saveData);
    } 

    private void LoadFromFile(string fullPath)
    {
        var response = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(fullPath));
        if (response == null)
        {
            Collective.Log.Error($"Failed to load save file {fullPath}");
            return;
        }

        _saveData = response;
    }

}