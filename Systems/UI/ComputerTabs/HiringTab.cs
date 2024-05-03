using System;
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions; 
using Collective.Components.Modals;
using Collective.Systems.Managers;
using Collective.Systems.UI.Forms;
using Collective.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Systems.UI.ComputerTabs;

public class HiringTab : UIParent
{
 
    private readonly Dictionary<Guid, GameObject> _employeeButtons = new();

    public void LoadData<T>(EventData<T> data) where T : class => UpdateView();

    public override void UpdateView() {
        Collective.Log.Info("Updating Hiring Tab Data");
        _employeeButtons.Clear();
        OnApplicantsUpdate();
        OnEmployeeUpdate(); 
        
        ThisObject.transform.GetChild(1).transform.GetChild(2).GetComponent<Toggle>().SetIsOnWithoutNotify(Collective.GetManager<GameDataManager>().GetApplicantSearchStatus());
        
    }
    
    private void Awake()
    {
        HiringPanelLoad();
        OnApplicantsUpdate();
        OnEmployeeUpdate(); 
    }

    private void HiringPanelLoad()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("HiringPanel", UIUtility.TabsObject().transform);
        if (ThisObject == null) return;

        ThisObject.transform.localPosition = UIUtility.TabsObject().transform.GetChild(1).localPosition;

        var font = UIUtility.GetPrimaryFont();
        ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
            .font = font;
        ThisObject.transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
            .font = font;
        ThisObject.transform.GetChild(1).transform.GetChild(2).GetComponent<Toggle>().onValueChanged
            .AddListener(UpdateApplicantSearchToggle);
        Hide();
    }


    private void AddStaff(Employee employee, ScrollRect parent, bool isEmployee = true)
    {
        if (_employeeButtons.ContainsKey(employee.Guid))
        {
            Debug.LogWarning($"Attempted to add duplicate button for GUID: {employee.Guid}");
            Debug.LogWarning($"Is Employee? {isEmployee}"); 
            return; // Skip adding if already exists
        }

        var button = UIUtility.LoadAsset<GameObject>("ApplicantButton", parent.content);
        if (button == null) return;

        button.transform.SetParent(parent.content, false);


        button.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = employee.Name;
        
        if (isEmployee)
        {
            var desiredSalary = "$" + employee.HourlyRate + "/hr " + employee.NextShift?.Open.ToString()  + " to " + employee.NextShift?.Close.ToString(); 
            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>()
                    .text = desiredSalary;
            
            var experienceText = button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            experienceText.fontStyle = FontStyles.Bold;
            experienceText.text = employee.IsCurrentlyWorking ? "Currently Working" : "Not Working";


            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                .text = employee.JobRole.ToString();
            
            // adjust labels

            button.transform.GetChild(1).transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                .text = "Current Role";

            button.transform.GetChild(1).transform.GetChild(2).transform.GetChild(1).GetComponent<TextMeshProUGUI>()
                .text = "Salary & Schedule";

            button.transform.GetChild(1).transform.GetChild(2).transform.GetChild(3).GetComponent<TextMeshProUGUI>()
                .text = "Status";
        }
        else
        {
            var jobRole = employee.DesiredJobRoles.First().ToString() ?? "Error";
            var desiredSalary = "$" + employee.DesiredHourlyRate + "/hr (" + employee.DesiredMinHoursWorked + "-" +
                                employee.DesiredMaxHoursWorked + ") hrs/day";
            var experience = employee.PastJobCount + " Previous Jobs";
            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text =
                jobRole; // Job role
            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>()
                    .text =
                desiredSalary;
            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(3).GetComponent<TextMeshProUGUI>()
                    .text =
                experience;
        }

        // Run through mood formatter to get color correct
        FormatMoodsWithColor(
            button.transform.GetChild(1).transform.GetChild(1).transform.GetChild(2).GetComponent<TextMeshProUGUI>(),
            employee.Mood);

        _employeeButtons.Add(employee.Guid, button.gameObject);
        ActivateButtonListeners(button, employee, isEmployee);

    }

    private void ActivateButtonListeners(GameObject button, Employee employee, bool isEmployee = true)
    {
        if (isEmployee)
        {
            button.transform.GetChild(3).gameObject.SetActive(true);
            var fireButton = button.transform.GetChild(3).transform.GetChild(1).GetComponent<Button>();
            fireButton.onClick.AddListener(() => FireEmployee(employee.Guid));
            var modifyRole = button.transform.GetChild(3).transform.GetChild(0).GetComponent<Button>();
            modifyRole.onClick.AddListener(() => ModifyRole(employee));
            return;
        }

        var dismissApplicant = button.transform.GetChild(2).transform.GetChild(1).GetComponent<Button>();
        dismissApplicant.onClick.AddListener(() => DismissApplicant(employee.Guid));

        var offerRole = button.transform.GetChild(2).transform.GetChild(0).GetComponent<Button>();
        offerRole.onClick.AddListener(() => MakeOffer(employee));
    }
    
    private void ModifyRole(Employee employee)
    {
        if (employee.IsCurrentlyWorking)
        {
            Collective.GetManager<UIManager>().ShowMessage("Cannot Modify Employee Role", "Cannot modify role while currently working");
            return;
        }
        
        var uiManager = Collective.GetManager<UIManager>(); 
        uiManager.LoadForm<ModifyRoleForm, Employee>("JobOfferForm", "Modify " + employee.Name + " role", employee);
    }

    private void MakeOffer(Employee employee)
    {
        var uiManager = Collective.GetManager<UIManager>(); 
        uiManager.LoadForm<MakeApplicantOfferForm, Employee>("JobOfferForm", "Create Offer for " + employee.Name, employee);
    }

    private void FireEmployee(Guid guid)
    {
        var staffManager = Collective.GetManager<StaffManager>();
        if (staffManager.GetEmployee(guid) == null) return;
        Collective.GetManager<UIManager>().Confirmation("Fire Employee", "Are you sure you want to fire this employee?",
            null, () =>
            {
                Collective.GetManager<StaffManager>().RemoveEmployee(guid);
                UpdateView();
                return true;
            });
    }

    private void DismissApplicant(Guid guid)
    {
        var staffManager = Collective.GetManager<StaffManager>();
        if (staffManager.GetApplicant(guid) == null) return;
        Collective.GetManager<UIManager>().Confirmation("Dismiss Applicant",
            "Remove the applicant from the pool of available workers?",
            null,
            () =>
            {
                Collective.GetManager<StaffManager>().RemoveApplicant(guid);
                UpdateView();
                return true;
            });
    }


    private void OnApplicantsUpdate()
    {
        if (ThisObject == null) return;
        var applicantPanel = ThisObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(1)
            .GetComponent<ScrollRect>();
        UIUtility.ClearScrollRect(applicantPanel);
        var manager = Collective.GetManager<StaffManager>();
        var applicants = manager.Applicants;
        applicants.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        applicants.ForEach(applicant => AddStaff(applicant, applicantPanel, false));
        applicantPanel.content.sizeDelta = new Vector2(applicantPanel.content.sizeDelta.x, 111 * applicants.Count);
        applicantPanel.verticalNormalizedPosition = 1.0f;
    }

    private void OnEmployeeUpdate() {
        if (ThisObject == null) return;
        var employeePanel = ThisObject.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1)
            .GetComponent<ScrollRect>();
        UIUtility.ClearScrollRect(employeePanel);
        var manager = Collective.GetManager<StaffManager>();
        var employees = manager.Employees;
        employees.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        employees.ForEach(employee => AddStaff(employee, employeePanel, true));
        employeePanel.content.sizeDelta = new Vector2(employeePanel.content.sizeDelta.x, 111 * employees.Count);   
        employeePanel.verticalNormalizedPosition = 1.0f;
    }

    private void UpdateApplicantSearchToggle(bool isChecked) =>
        Collective.GetManager<GameDataManager>().SetAcceptingApplicants(isChecked);

 
    
    private static void FormatMoodsWithColor(TMP_Text textComponent, IReadOnlyCollection<Mood> moods)
    {
        if (moods.Count == 0)
        {
            textComponent.text = "Unknown";
            textComponent.color = Color.gray;
            return;
        }

        // Determine the overall mood sentiment
        var moodScore = moods.Sum(mood => (int)mood); 

        // Assuming there are max 2 moods and each has a value from Happy (0) to Exhausted (5)
        var maxScore = 10; // Max score for 2 moods at their worst (5 each)
        var third = maxScore / 3;

        // Assign color based on the aggregated mood score
        if (moodScore <= third)
            textComponent.color = new Color(130,97,255);
        else if (moodScore <= 2 * third)
            textComponent.color = new Color(215,99,39);
        else
            textComponent.color = new Color(215,85,72);
  
        textComponent.text = string.Join(" - ", moods.Select(m => m.ToString()));
    }
}