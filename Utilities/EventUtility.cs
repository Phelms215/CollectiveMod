using System;
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Modals;

namespace Collective.Utilities;

public static class EventUtility
{
    private static readonly Dictionary<ModEventType, List<Delegate>> EventHandlers = new();
    public static void Invoke<T>(ModEventType type, T eventData) where T : class
    {
        if (!EventHandlers.ContainsKey(type) || EventHandlers[type] == null || !EventHandlers[type].Any()) 
            return; 
        
        
        var handlers = EventHandlers[type];
        var eventDataWrapper = new EventData<T>(eventData, type);
        Collective.Log.Info($"Invoking event {type}");
        foreach (var handler in handlers.ToList()) // ToList to avoid modification during iteration
        {
            try
            {
                handler.DynamicInvoke(null, eventDataWrapper);
            }
            catch (Exception ex)
            {           
                var targetType = handler.Target?.GetType().Name ?? "Static Method";
                var methodName = handler.Method.Name;
                
                // Log the exception with details about the handler, class and method
                Collective.Log.Error($"Error invoking event handler for {type}: {ex.InnerException?.Message ?? ex.Message}, " +
                                     $"Handler: {targetType}.{methodName}");
                Collective.Log.Error(ex.StackTrace);
            }
        }  
    }

    public static void Invoke<T>(ModEventType type, T eventData, Action finalAction) where T : class
    {
        if (!EventHandlers.ContainsKey(type) || EventHandlers[type] == null || !EventHandlers[type].Any())
        {
            finalAction.Invoke(); // Optionally invoke the final action if no handlers are present
            return;
        }
        Invoke<T>(type, eventData);
        finalAction.Invoke(); // Invoke the final action after all handlers have executed
    }
    
    public static void Subscribe<T>(ModEventType type, EventHandler<EventData<T>> handler) where T : class
    {
        if (!EventHandlers.ContainsKey(type))
            EventHandlers[type] = new List<Delegate>();

        EventHandlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(ModEventType type, EventHandler<EventData<T>> handler) where T : class
    {
        if (!EventHandlers.ContainsKey(type))
            return;

        EventHandlers[type].Remove(handler);
    } 
    
    // Method to remove all event subscriptions
    public static void ClearAllSubscriptions() => EventHandlers.Clear(); 
}