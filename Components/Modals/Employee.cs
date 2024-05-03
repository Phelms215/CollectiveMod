using System;
using System.Collections.Generic;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Definitions;
using JetBrains.Annotations;
using UnityEngine; 
using Vector3 = UnityEngine.Vector3;

namespace Collective.Components.Modals;

[Serializable]
public class Employee : IGameEntity
{
    [SerializeField] public Guid Guid { get; } = Guid.NewGuid();
    [SerializeField] public string Name { get; set; } = "";
    [CanBeNull] [SerializeField] public float HourlyRate { get; set; }
    [CanBeNull] [SerializeField] public JobRole JobRole { get; set; }

    [SerializeField] public float PositionX { get; set; } = 0;
    [SerializeField] public float PositionY { get; set; } = 0;
    [SerializeField] public float PositionZ { get; set; } = 0;
    [SerializeField] public string PrefabName { get; set; } = ""; // New field to store the prefab name

    [SerializeField] public int DesiredHourlyRate { get; set; }
    [SerializeField] public int HiredDate { get; set; }
    [SerializeField] public int DesiredMinHoursWorked { get; set; }
    [SerializeField] public int DesiredMaxHoursWorked { get; set; }
    [SerializeField] public int PastJobCount { get; set; }

    [SerializeField] public List<Traits> Traits { get; set; } = new();
    [SerializeField] public List<JobRole> DesiredJobRoles { get; set; } = new();
    [SerializeField] public List<Mood> Mood { get; set; } = new List<Mood>();

    [SerializeField] public bool IsCurrentlyWorking { get; set; }
    [SerializeField] public StoreHours? NextShift { get; set; }

    [SerializeField] public int HoursSinceLastBreak { get; set; }

    public Vector3 GamePosition()
    {
        return new Vector3(PositionX, PositionY, PositionZ);
    }

    public void SetGamePosition(Vector3 position)
    {
        PositionX = position.x;
        PositionY = position.y;
        PositionZ = position.z;
    }

}