using System;
using System.Collections.Generic;
using Collective.Components.Modals;
using Collective.Definitions;
using UnityEngine;

namespace Collective.Components.DataSets;

[Serializable]
public class SaveData
{
    [SerializeField] private List<string> unlockedPermits = new List<string>();
    [SerializeField] private List<Employee> employees = new List<Employee>();
    [SerializeField] private List<Employee> applicants = new List<Employee>();
    [SerializeField] private List<Distributor> distributors = new List<Distributor>();
    [SerializeField] private MiscSettings settings = new MiscSettings();
    [SerializeField] private List<int> productsSold = new List<int>();
    
    public MiscSettings Settings => settings;
    public List<string> UnlockedPermits => unlockedPermits;
    public void UpdatePermits(List<string> permits) => unlockedPermits = permits;

    public List<Employee> Employees
    {
        get => employees;
        set => employees = value;
    }

    public List<Employee> Applicants
    {
        get => applicants;
        set => applicants = value;
    }

    public List<Distributor> Distributors
    {
        get => distributors;
        set => distributors = value;
    }

    public void UpdateTime(int hour, int minute) => settings.CurrentTime = new Hours(hour, minute);

    public void MarkProductSold(int productId)
    {   
        if(productsSold.Contains(productId)) return;
        productsSold.Add(productId);
    }
    
    

    public List<string> GetUnlockedPermits() => unlockedPermits;
    public void SetAutoOpen(bool autoOpen) => settings.AutoOpen = autoOpen;
    public void SetSearchForApplicants(bool searchForApplicants) => settings.SearchForApplicants = searchForApplicants;
    public void SetStoreHours(StoreHours hours) => settings.StoreHours = hours;
    
    public void SetStoreStatus(bool isOpen) => settings.StoreOpen = isOpen;

    public void SetPermitUnlocked(List<string> permits)
    {

        unlockedPermits = permits;
    }

    public void UpdatePlayerPosition()
    {
        var position = GameObject.Find("Player").transform.position;
        var rotation = GameObject.Find("Player").transform.rotation;
        settings.PlayerX = position.x;
        settings.PlayerY = position.y;
        settings.PlayerZ = position.z;
        settings.PlayerRotationX = rotation.x;
        settings.PlayerRotationY = rotation.y;
        settings.PlayerRotationZ = rotation.z; 
        settings.PlayerRotationW = rotation.w;
    }
    
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(settings.PlayerX, settings.PlayerY, settings.PlayerZ);
    }

    public Quaternion GetPlayerRotation()
    {
        return new Quaternion(settings.PlayerRotationX, settings.PlayerRotationY, settings.PlayerRotationZ, settings.PlayerRotationW);
    }
    [Serializable]
    public class MiscSettings {
        
        [SerializeField] 
        public StoreHours StoreHours { get; set; } = new StoreHours(Key.DefaultStoreOpenHour,
            Key.DefaultStoreOpenMinute, Key.DefaultStoreCloseHour, Key.DefaultStoreCloseMinute);

        [SerializeField] public bool StoreOpen { get; set; } = false;
        [SerializeField] public bool LightOn { get; set; } = false;
        [SerializeField] public bool AutoOpen { get; set; } = true;
        [SerializeField] public bool SearchForApplicants { get; set; } = true;
        [SerializeField] public Hours CurrentTime { get; set; } = new Hours(8, 0);
        
        [SerializeField] public float PlayerRotationX { get; set; } = 0;
        [SerializeField] public float PlayerRotationY { get; set; } = 0;
        [SerializeField] public float PlayerRotationZ { get; set; } = 0;

        [SerializeField] public float PlayerRotationW { get; set; } = 0;
        [SerializeField] public float PlayerX { get; set; } = 0;
        [SerializeField] public float PlayerY { get; set; } = 0;
        [SerializeField] public float PlayerZ { get; set; } = 0;
    }
}