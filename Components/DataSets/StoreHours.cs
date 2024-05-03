namespace Collective.Components.DataSets;

public class StoreHours
{
    public Hours Open { get; set; }
    public Hours Close { get; set; }

    // Parameterless constructor for serialization
    public StoreHours()
    {
        
    }

    public StoreHours(int openHour, int openMinute, int closeHour, int closeMinute)
    {
        Open = new Hours(openHour, openMinute);
        Close = new Hours(closeHour, closeMinute);
    }

    public StoreHours(Hours open, Hours close)
    {
        Open = open;
        Close = close;
    }


    public bool OutsideHours(Hours currentTime)
    {
        // Convert Hours and Minutes to total minutes for easier comparison
        var openMinutes = (Open.Hour - 1) * 60 + Open.Minute;
        var closeMinutes = Close.Hour * 60 + Close.Minute;
        var currentMinutes = currentTime.Hour * 60 + currentTime.Minute;

        // Check for overnight closing scenario
        if (closeMinutes < openMinutes)
            // Store closes after midnight
            // Outside hours if current time is after close and before open
            return currentMinutes > closeMinutes && currentMinutes < openMinutes;

        // Store closes the same day it opens
        // Outside hours if current time is before open or after close
        return currentMinutes < openMinutes || currentMinutes > closeMinutes;

    }

    public override string ToString()
    {
        return $"Open: {Open}, Close: {Close}";
    }

}