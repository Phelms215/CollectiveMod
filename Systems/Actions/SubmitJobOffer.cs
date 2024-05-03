using System;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Systems.Actions;

public class SubmitJobOffer : IGameAction<ApplicantFormSubmission>
{
    private readonly Random _rng = new Random();
    
    public void Execute(ApplicantFormSubmission submission)
    {
        var dataManager = Collective.GetManager<StaffManager>();
        var employee = submission.Employee;

        if (ShouldAcceptJob(employee, submission))
        {
            dataManager.HireEmployee(UpdateRecord(employee, submission));
            Collective.GetManager<UIManager>().DisplayMessage($"{employee.Name} Accepted your job offer!");
            Collective.Log.Info($"Job offer accepted by {employee.Name}");
        }
        else
        {
            dataManager.RemoveApplicant(employee.Guid);
            Collective.Log.Info($"Job offer rejected by {employee.Name}");
            Collective.GetManager<UIManager>().DisplayMessage($"Job offer rejected by {employee.Name}"); 
        }
        Collective.GetManager<UIManager>().RefreshTab(Views.Hiring);
    }

    private Employee UpdateRecord(Employee employee, ApplicantFormSubmission submission)
    { 
        employee.NextShift = new StoreHours(new Hours(submission.StartTimeHour, submission.StartTimeMinute), new Hours(submission.EndTimeHour, submission.EndTimeMinute));
        employee.IsCurrentlyWorking = false;
        employee.HoursSinceLastBreak = 0;
        employee.HiredDate = Singleton<DayCycleManager>.Instance.CurrentDay;
        employee.HourlyRate = submission.HourlyRate;
        employee.JobRole = submission.Role;
        return employee;
    }

    private bool ShouldAcceptJob(Employee employee, ApplicantFormSubmission submission)
    {
        Collective.Log.Info($"Job offer submitted to {employee.Name}"); 
        
        var roleMatch = employee.DesiredJobRoles.Contains(submission.Role);
        if(!roleMatch) return RejectReason(RejectReasons.Role);

        var rateAcceptable = submission.HourlyRate >= (employee.DesiredHourlyRate * .75f);
        if(!rateAcceptable) return RejectReason(RejectReasons.Rate);
            
            
        var totalHoursOffered = submission.EndTimeHour - submission.StartTimeHour;
        
        var adjustedMinHours = employee.DesiredMinHoursWorked * .75f;
        var adjustedMaxHours = employee.DesiredMaxHoursWorked * UnityEngine.Random.Range(1.25f, 2f);
         
        var hoursAcceptable = (totalHoursOffered >= adjustedMinHours && totalHoursOffered <= adjustedMaxHours);
        if(!hoursAcceptable) return RejectReason(RejectReasons.TotalHours);
        
        return true;

    }

    private enum RejectReasons
    {
        Rate,
        TotalHours,
        Role
    }

    private bool RejectReason(RejectReasons reason)
    {
        Collective.Log.Info($"Job offer rejected for {reason}");
        return false;
    }

 
    private int GetTotalHours(int startHour, int endHour)
    {
        if (endHour >= startHour)
        {
            return endHour - startHour;
        }
        return (24 - startHour) + endHour; // Handling shift that crosses midnight
    }
}
