using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Collective.Components.DataSets;
using Collective.Systems.Actions;
using Collective.Systems.Managers;
using Collective.Utilities;

namespace Collective.Systems.UI.Forms;

public class MakeApplicantOfferForm : IOverlayForm<Employee>
{
    private TMP_InputField _hourlyRate;
    private TMP_InputField _startTimeHour;
    private TMP_InputField _startTimeMinute;
    private TMP_InputField _endTimeHour;
    private TMP_InputField _endTimeMinute;
    private TMP_Dropdown _roleDropdown;
    
    private Employee _employee;

    public void Load(Employee employee, GameObject formObject)
    {
        _employee = employee;
        // Initialize components
        
        _hourlyRate = formObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(1)
            .GetComponent<TMP_InputField>();
        _startTimeHour = formObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(2)
            .GetComponent<TMP_InputField>();
        _startTimeMinute = formObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(1)
            .GetComponent<TMP_InputField>();
        _endTimeHour = formObject.transform.GetChild(0).transform.GetChild(2).transform.GetChild(2)
            .GetComponent<TMP_InputField>();
        _endTimeMinute = formObject.transform.GetChild(0).transform.GetChild(2).transform.GetChild(1)
            .GetComponent<TMP_InputField>();
        _roleDropdown = formObject.transform.GetChild(0).transform.GetChild(3).transform.GetChild(1)
            .GetComponent<TMP_Dropdown>(); 
        

        // Subscribe to changes
        _hourlyRate.onEndEdit.AddListener(ValidateHourlyRate);
        _startTimeHour.onEndEdit.AddListener(ValidateStartTime);
        _startTimeMinute.onEndEdit.AddListener(ValidateStartTime);
        _endTimeHour.onEndEdit.AddListener(ValidateEndTime);
        _endTimeMinute.onEndEdit.AddListener(ValidateEndTime);
    }

    private void ValidateHourlyRate(string arg)
    {
        Debug.Log("Validating hourly rate");  // Debug statement
        if (float.TryParse(_hourlyRate.text, out float rate) && rate >= 0)
        {
            _hourlyRate.text = rate.ToString("F2");
        }
        else
        {
            _hourlyRate.text = "1.00";
        }
    }

    private void ValidateStartTime(string arg0)
    {
        _startTimeHour.text = EnsureTwoDigits(_startTimeHour.text);
        _startTimeMinute.text = EnsureTwoDigits(_startTimeMinute.text);
        
        ValidateTimeOrder();
    }

    private void ValidateEndTime(string arg0)
    {
        _endTimeHour.text = EnsureTwoDigits(_endTimeHour.text);
        _endTimeMinute.text = EnsureTwoDigits(_endTimeMinute.text);
        ValidateTimeOrder();
    }

    private string EnsureTwoDigits(string input)
    {
        return int.TryParse(input, out int number) ? number.ToString("D2") : "00";
    }

    private void ValidateTimeOrder()
    {
        if (TimeSpan.TryParse($"{_startTimeHour.text}:{_startTimeMinute.text}", out TimeSpan startTime) &&
            TimeSpan.TryParse($"{_endTimeHour.text}:{_endTimeMinute.text}", out TimeSpan endTime))
        {
            if (startTime >= endTime)
            {
                _endTimeHour.text = startTime.Add(TimeSpan.FromHours(1)).ToString("hh");
                _endTimeMinute.text = startTime.ToString("mm");
            }
        }
    }

    
    public bool SubmitButtonHandler()
    { 
 
        var startTimeHour = int.Parse(_startTimeHour.text);
        var startTimeMinute = int.Parse(_startTimeMinute.text);
        var endTimeHour = int.Parse(_endTimeHour.text);
        var endTimeMinute = int.Parse(_endTimeMinute.text);
        
        // Calculate the minutes since start of the day for both start and end times
        int startMinutes = startTimeHour * 60 + startTimeMinute;
        int endMinutes = endTimeHour * 60 + endTimeMinute;

        // Calculate total working time in minutes
        int totalMinutesWorked;
        if (endMinutes >= startMinutes)
        {
            totalMinutesWorked = endMinutes - startMinutes;
        }
        else
        {
            // Handle shift going past midnight
            totalMinutesWorked = (1440 - startMinutes) + endMinutes; // 1440 minutes in a day
        }

        // Convert minutes to hours and check if greater than 8 hours
        if (totalMinutesWorked > 480) // 480 minutes is 8 hours
        {
            Collective.GetManager<UIManager>()
                .ShowMessage("Error Creating Job Offer", "Employees cannot work more than 8 hours.");
            return false;
        } 
        
        var finalRecord = new ApplicantFormSubmission(
            hourlyRate: float.Parse(_hourlyRate.text),
            startTimeHour: int.Parse(_startTimeHour.text),
            startTimeMinute: int.Parse(_startTimeMinute.text),
            endTimeHour: int.Parse(_endTimeHour.text),
            endTimeMinute: int.Parse(_endTimeMinute.text),
            role: (JobRole)_roleDropdown.value,
            employee: _employee 
        );
        
        ActionUtility.Run<SubmitJobOffer, ApplicantFormSubmission>(finalRecord);
        return true;
    } 
    

    public bool CancelButtonHandler()
    {
        Collective.Log.Info("Cancellation by user.");
        return true;
    }
}
