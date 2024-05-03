using Collective.Components.Modals;
using Collective.Definitions;

namespace Collective.Components.DataSets;
public class ApplicantFormSubmission
{
    public ApplicantFormSubmission(float hourlyRate, int startTimeHour, int startTimeMinute, int endTimeHour, int endTimeMinute, JobRole role, Employee employee)
    {
        this.HourlyRate = hourlyRate;
        this.StartTimeHour = startTimeHour;
        this.StartTimeMinute = startTimeMinute;
        this.EndTimeHour = endTimeHour;
        this.EndTimeMinute = endTimeMinute;
        this.Role = role;
        this.Employee = employee;
    }

    public float HourlyRate { get; }
    public int StartTimeHour { get; }
    public int StartTimeMinute { get; }
    public int EndTimeHour { get; }
    public int EndTimeMinute { get; }
    public JobRole Role { get; }
    public Employee Employee { get; }
}
