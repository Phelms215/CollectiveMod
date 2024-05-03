using System;
using UnityEngine;

namespace Collective.Components.DataSets;

[Serializable]
public class Hours
{
    // 24 hour format
    [SerializeField] public int Hour { get; set;  }
    [SerializeField] public  int Minute { get; set; }
   
    public Hours(int hour, int minute)
    {
        Hour = hour;
        Minute = minute;
    }

    public bool Equals(Hours obj)
    {
        if(Hour!= obj.Hour) return false;
        if(Minute!= obj.Minute) return false;
        return true;
    }
    

    public int HoursSince(Hours obj)
    {
        // Convert both times to minutes since midnight
        int totalCurrentMinutes = Hour * 60 + Minute;
        int totalObjMinutes = obj.Hour * 60 + obj.Minute;

        // Calculate the difference in minutes
        int minutesDifference = totalCurrentMinutes - totalObjMinutes;

        // Convert minutes difference to hours, discarding any remaining minutes
        return minutesDifference / 60;
    }

    public bool HasPassed(Hours obj)
    {
        // Convert the times of the current instance and the parameter to minutes since midnight
        int totalCurrentMinutes = Hour * 60 + Minute;
        int totalObjMinutes = obj.Hour * 60 + obj.Minute;

        // Check if the current instance represents a time later than the parameter's time
        return totalCurrentMinutes > totalObjMinutes;
    }

    
    public bool IsBetween(Hours startTime, Hours endTime)
    {
        // Convert Hours and Minutes to total minutes for easier comparison
        var startMinutes = startTime.Hour * 60 + startTime.Minute;
        var endMinutes = endTime.Hour * 60 + endTime.Minute;
        var currentMinutes = Hour * 60 + Minute;

        if (startMinutes <= endMinutes) 
            return currentMinutes >= startMinutes && currentMinutes <= endMinutes; 
        
        // Handle overnight scenarios
        return currentMinutes >= startMinutes || currentMinutes <= endMinutes;
    }

    public new string ToString()
    {
        return Hour.ToString("00") + ":" + Minute.ToString("00");
    }
}