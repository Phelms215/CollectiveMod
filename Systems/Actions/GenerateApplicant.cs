using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Managers;
using MyBox;

namespace Collective.Systems.Actions;


public class GenerateApplicant : IGameAction
{
    private readonly string[] Names =
    {
        "Alex", "Jordan", "Taylor", "Morgan", "Casey",
        "Jamie", "Riley", "Cameron", "Sam", "Dakota",
        "Charlie", "Quinn", "Skyler", "Avery", "Reese",
        "Peyton", "Kai", "Rowan", "Finley", "Emerson",
        "Harper", "Blake", "Elliot", "Reagan", "Logan",
        "Madison", "Bailey", "Corey", "Drew", "Alexis",
        "Hayden", "Addison", "Spencer", "Parker", "Kelly",
        "Dylan", "Leslie", "Tyler", "Sydney", "Brooke",
        "Ryan", "Kendall", "Pat", "Terry", "Shannon",
        "Rene", "Jessie", "Chris", "Ariel", "Devon",
        "Frankie", "Robin", "Sky", "Brett", "Kerry",
        "Dana", "Kim", "Shawn", "Lynn", "Adrian",
        "Stacy", "Lee", "Shane", "Gabriel", "Tracey",
        "Jody", "Ash", "Randy", "Bobbie", "Nicky",
        "Jaden", "River", "Sage", "Eden", "Sloane",
        "Blair", "Sawyer", "London", "Phoenix", "Dallas",
        "Denver", "Cody", "Sutton", "Presley", "Tatum",
        "Marley", "Harley", "Kennedy", "Lane", "Rory",
        "Milan", "Remy", "Teagan", "Justice", "Kendrick",
        "Ellis", "Arden", "Blaine", "Dakari", "Easton"
    };

    private readonly System.Random RNG = new System.Random();

    public void Execute()
    {
        if (!ShouldSearchForApplicants()) return;
        Collective.Log.Info("Searching for applicants");
        Generate();
    }


    private bool ShouldSearchForApplicants()
    {
        // Are we even hiring? 
        if (!Collective.GetManager<GameDataManager>().GetSaveData().Settings.SearchForApplicants) return false;
         
        // For now just some rng - 40% chance to search for applicants 
        var randomNumber = RNG.Next(1, 100); 
        if(randomNumber < 60) return false;

        // Make sure we don't have too many applicants
        var applicants = Collective.GetManager<StaffManager>().Applicants; 
        return applicants.Count < 10;
    }

    private void Generate()
    {
        var traitList = new List<Traits> { Traits.Honest };
 
        var minHours = UnityEngine.Random.Range(1, 3);
        var maxHours = UnityEngine.Random.Range(4, 8);

        // Ensure minHours is not greater than maxHours
        if (minHours > maxHours)
        {
            (minHours, maxHours) = (maxHours, minHours);
        }
        // Get the list of prefabs
        var prefabs = CustomerGenerator.Instance.m_CustomerPrefabs;
        // Select a random prefab
        var selectedPrefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
        var prefabName = selectedPrefab.name;  // Assuming there is a 'name' field available
        
        Employee applicant = new Employee
        {
            Name = Names[UnityEngine.Random.Range(0, Names.Length)],
            HourlyRate = 0,
            DesiredHourlyRate = UnityEngine.Random.Range(10, 30),
            DesiredMinHoursWorked = minHours,
            DesiredMaxHoursWorked = maxHours,
            PastJobCount = UnityEngine.Random.Range(0, 10),
            Traits = GenerateRandomTraits(),
            PrefabName = prefabName,
            DesiredJobRoles = new List<JobRole>
            {
                (JobRole)UnityEngine.Random.Range(0, Enum.GetNames(typeof(JobRole)).Length)
            },
            IsCurrentlyWorking = false,
        };
 
        var staffManager = Collective.GetManager<StaffManager>();
        staffManager.AddApplicant(applicant);
        Collective.GetManager<UIManager>().RefreshTab(Views.Hiring);
    }


    private List<Traits> GenerateRandomTraits()
    {
        var conflictingTraits = new Dictionary<Traits, Traits>
        {
            { Traits.Liar, Traits.Honest },
            { Traits.Thief, Traits.Kind },
            { Traits.Optimistic, Traits.Pessimistic }
        };

        List<Traits> traitsList = new List<Traits>();
        var availableTraits = Enum.GetValues(typeof(Traits)).Cast<Traits>().ToList();

        int traitsCount = UnityEngine.Random.Range(1, availableTraits.Count + 1); // Random number of traits to add

        while (traitsList.Count < traitsCount)
        {
            if (availableTraits.Count == 0) break; // Break if no available traits are left to add

            Traits randomTrait = availableTraits[UnityEngine.Random.Range(0, availableTraits.Count)];
            traitsList.Add(randomTrait);

            // Remove the added trait and its conflicting trait from available traits to prevent conflicts
            availableTraits.Remove(randomTrait);
            if (conflictingTraits.ContainsKey(randomTrait) && availableTraits.Contains(conflictingTraits[randomTrait]))
            {
                availableTraits.Remove(conflictingTraits[randomTrait]);
            }
        }

        return traitsList;
    }

}