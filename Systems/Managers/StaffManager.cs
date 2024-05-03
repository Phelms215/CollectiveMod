using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Actions;
using Collective.Systems.Entities;
using Collective.Utilities;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Collective.Systems.Managers;

public class StaffManager : ManagerUtility, IManage, ITick
{
    public List<Employee> Employees { get; private set; } = new();
    public List<Employee> Applicants { get; private set; } = new();
    public List<RestockerTask> RestockerTasks { get; private set; } = new();
    private readonly Dictionary<Guid, StaffMember> _staffMembers = new(); 
 
    public Employee? GetEmployee(Guid employeeId) => Employees.FirstOrDefault(x => x.Guid == employeeId);

    public Employee? GetApplicant(Guid employeeId) => Applicants.FirstOrDefault(x => x.Guid == employeeId);

    public void Tick()
    { 
        ActionUtility.Run<CheckEmployeeShouldSpawn>();
        if (Random.Range(0, 100) < 10) ActionUtility.Run<GenerateApplicant>();
        
        for (var i = 0; i < _staffMembers.Values.Count; i++)
        {
            var thisStaffMember = _staffMembers.Values.ElementAt(i);
            thisStaffMember.ShouldGoHome(); 
        } 
        ActionUtility.Run<GenerateRestockerTasks>();
    }

    public void UpdateRestockerTasks(List<RestockerTask> restockerTasks)
    {
        restockerTasks.ForEach(newTask =>
        {
            var currentTasks = RestockerTasks.FirstOrDefault(x => newTask.TargetDisplaySlot == x.TargetDisplaySlot);
            if (currentTasks == null)
            {
                RestockerTasks.Add(newTask);
                return;
            }
            if(currentTasks.IsAssigned || currentTasks.HasStarted) return; 
            RestockerTasks.Remove(currentTasks);
            RestockerTasks.Add(newTask);
        });
    }

    public RestockerTask? FetchNewTask(Employee employee, int productID = 0)
    {
        RestockerTask? thisTask = null;
        if(productID != 0) 
            thisTask = RestockerTasks.FirstOrDefault(i => i.TargetDisplaySlot.ProductID == productID && i.IsAssigned == false && i.HasStarted == false);
        else
            thisTask = RestockerTasks.FirstOrDefault(i => i.IsAssigned == false && i.HasStarted == false);
        
        if (thisTask == null) return null;
        
        thisTask.AssignedEmployee = employee;
        thisTask.IsAssigned = true;
        thisTask.HasStarted = true;
        return thisTask;
    }

    public void RecordEmployeePosition()
    {
        foreach (var staffMembersValue in _staffMembers.Values)
            Employees.FirstOrDefault(i => i.Guid == staffMembersValue.Guid)
                ?.SetGamePosition(staffMembersValue.GetStaffMemberPosition());
    }

    public void CompleteTask(Guid guid)
    {
        var task = RestockerTasks.FirstOrDefault(x => x.ID == guid);
        if (task == null) return;
        RestockerTasks.Remove(task);
    }

    public void CompleteAnimation(Cashier cashier, int animation)
    {
        _staffMembers.ForEach(x => x.Value.CompleteAnimation(cashier, animation));
    }

    public void SpawnEmployee(Employee employee)
    {
        if(_staffMembers.ContainsKey(employee.Guid)) return;
        var gameObject = new GameObject("Collective-StaffMember-" + employee.Guid).AddComponent<StaffMember>();
        gameObject.Setup(employee, _staffMembers.Count + 1);
        gameObject.GoToWork();
        Collective.GetManager<UIManager>().ShowToast(employee.Name + "has started their shift");
        _staffMembers.Add(employee.Guid, gameObject);
        
    }

    public void RemoveStaff(Guid guid)
    {
        var employee = GetEmployee(guid); 
        if(employee?.NextShift == null) return;
        var hasPassed = employee.NextShift.Close.HoursSince(employee.NextShift.Open);
        var cost = employee.HourlyRate * hasPassed;

        Singleton<MoneyManager>.Instance.MoneyTransition(cost, MoneyManager.TransitionType.STAFF);
        Collective.GetManager<UIManager>().ShowToast(employee.Name + " has ended their shift total salary paid was $" + cost);
        _staffMembers.Remove(guid);
    }
    
     
    // Task my Restocker Guid 
    public RestockerTask? GetRestockerTaskById(Guid taskId) => RestockerTasks.FirstOrDefault(x => x.ID == taskId);
 
    
    public void AddEmployee(Employee employee)
    {
        var localEntry = Employees.FirstOrDefault(e => e.Guid == employee.Guid);
        if (localEntry != null)
            Employees.Remove(localEntry);
        Employees.Add(employee);
    }


    public void HireEmployee(Employee employee)
    {
        
        if(Employees.FirstOrDefault(e => e.Guid == employee.Guid)!= null) return;
        var localEntry = Applicants.FirstOrDefault(e => e.Guid == employee.Guid); 
            Applicants.Remove(localEntry);
        
        Employees.Add(employee);
    }

    public void AddApplicant(Employee applicant)
    {
        var localEntry = Applicants.FirstOrDefault(e => e.Guid == applicant.Guid);
        if (localEntry != null)
            Applicants.Remove(localEntry);
        Applicants.Add(applicant);
    }
    
    public void RemoveApplicant(Guid guid) => Applicants.Remove(Applicants.FirstOrDefault(x => x.Guid == guid));
    public void RemoveEmployee(Guid guid) => Employees.Remove(Employees.FirstOrDefault(x => x.Guid == guid));



    protected override void LoadInitialData(object sender, EventData<SaveData> saveData)
    {
 
        Employees = saveData.Data.Employees;
        Applicants = saveData.Data.Applicants;
        Employees.Where(x => x.IsCurrentlyWorking == true).ToList().ForEach(employee =>
        {
            if (_staffMembers.ContainsKey(employee.Guid)) return;
            var gameObject = new GameObject("Collective-StaffMember-" + employee.Guid).AddComponent<StaffMember>();
            gameObject.Setup(employee, _staffMembers.Count + 1);
            gameObject.ReloadTask();
            _staffMembers.Add(employee.Guid, gameObject);
        });

    } 

 

}