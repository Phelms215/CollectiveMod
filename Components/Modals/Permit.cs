using Collective.Definitions;

namespace Collective.Components.Modals;

public class Permit
{
    public readonly string ID;
    public string Title { get; set; }
    public string Description { get; set; }
    
    public PermitType Type { get; set; }
    public int Level { get; set; }
    public int Cost { get; set; }
    public string[] PreRequirements { get; set; }

    public Permit(string id)
    {
        ID = id;
    }
    
}