using System; 
using System.Linq; 
using Collective.Components.Interfaces;
using Collective.Components.Modals; 
using Collective.Systems.Managers;
using MyBox;  
namespace Collective.Systems.Actions;

public class CheckEmployeeShouldSpawn : IGameAction
{ 
    public void Execute()
    {
        var staffManager = Collective.GetManager<StaffManager>();
        var employeeList = staffManager.Employees;
        var currentHour = Singleton<DayCycleManager>.Instance.CurrentHour;
        var currentMinute = Singleton<DayCycleManager>.Instance.CurrentMinute;

        // Convert AM/PM to a 24-hour format for easier comparison
        if (!Singleton<DayCycleManager>.Instance.AM)
            currentHour += 12;
        else if (currentHour == 12 && Singleton<DayCycleManager>.Instance.AM)
            currentHour = 0;

        var actions = (from employee in employeeList.Where(employee => !employee.IsCurrentlyWorking) where ShouldGoToWork(employee, currentHour, currentMinute) select (Action)(() => staffManager.SpawnEmployee(employee))).ToList(); // List to hold actions

        // Execute all collected actions
        foreach (var action in actions)
            action();
    }

    private bool ShouldGoToWork(Employee employee, int currentHour, int currentMinute)
    {
        if (employee.NextShift == null)
        {
            Collective.Log.Info($"Employee {employee.Name} has no next shift");
            return false;
        }

        if (employee.HiredDate == Singleton<DayCycleManager>.Instance.CurrentDay) return false;
        
        var shiftStartHour = employee.NextShift.Open.Hour;
        var shiftStartMinute = employee.NextShift.Open.Minute;
        var shiftEndHour = employee.NextShift.Close.Hour;
        var shiftEndMinute = employee.NextShift.Close.Minute;

        // Calculate the time 10 minutes before the shift starts
        var adjustedStartMinute = shiftStartMinute - 10;
        var adjustedStartHour = shiftStartHour;
        if (adjustedStartMinute < 0)
        {
            adjustedStartMinute += 60;  // Adjust the minute upward
            adjustedStartHour -= 1;     // Decrement the hour
            if (adjustedStartHour < 0)  // Handle the midnight wrap-around
            {
                adjustedStartHour = 23;
            }
        }

        var shiftEndTimeInMinutes = shiftEndHour * 60 + shiftEndMinute;
        var currentMinuteFromMidnight = currentHour * 60 + currentMinute;
        var adjustedStartMinuteFromMidnight = adjustedStartHour * 60 + adjustedStartMinute;

        // Check if the current time is between the adjusted start time and the end time
        return currentMinuteFromMidnight >= adjustedStartMinuteFromMidnight && currentMinuteFromMidnight <= shiftEndTimeInMinutes;
    }



}