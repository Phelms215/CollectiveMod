namespace Collective.Components.DataSets;

public class ShippingCost
{
    public float SameDay { get; set; }
    public float NextDay { get; set; }
    public float TwoDays { get; set; }

    public ShippingCost(float sameDay, float nextDay, float twoDays)
    {
        SameDay = sameDay;
        NextDay = nextDay;
        TwoDays = twoDays;
    }
}