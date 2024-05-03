using System;
using Collective.Components.DataSets;
using Collective.Components.Interfaces;

namespace Collective.Utilities;

public static class ActionUtility
{
    public static ActionResultRecord<TReturn> Run<TAction, TArg, TReturn>(TArg arg)
        where TAction : IGameAction<TArg>, new()
        where TArg : class
        where TReturn : class
    {
        try
        {
            var action = new TAction();
            return new ActionResultRecord<TReturn>(action.Execute<TReturn>(arg));
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to execute action {typeof(TAction).Name}: {ex}";
            Collective.Log.Error(errorMessage);
            return new ActionResultRecord<TReturn>(null, false, errorMessage);
        }
    }
    
    public static ActionResultRecord Run<TAction, TArg>(TArg arg)
        where TAction : IGameAction<TArg>, new()
        where TArg : class 
    {
        try
        {
            var action = new TAction();
            action.Execute(arg);
            return new ActionResultRecord();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to execute action {typeof(TAction).Name}: {ex}";
            Collective.Log.Error(errorMessage);
            return new ActionResultRecord(false, errorMessage);
        }
    }
    
    public static ActionResultRecord Run<TAction>()
        where TAction : IGameAction, new()
    {
        try
        {
            var action = new TAction();
            action.Execute();
            return new ActionResultRecord();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to execute action {typeof(TAction).Name}: {ex}";
            Collective.Log.Error(errorMessage);
            return new ActionResultRecord(false, errorMessage);
        }
    }
    
    public static void RunSilent<TAction, TArg>(TArg arg)
        where TAction : IGameAction<TArg>, new()
        where TArg : class 
    {
        try
        {
            var action = new TAction();
            action.Execute(arg); 
        }
        catch (Exception ex)
        {
            Collective.Log.Error($"Failed to execute action {typeof(TAction).Name}: {ex}");
        }
    } 
    
    public static void RunSilent<TAction>()
        where TAction : IGameAction, new()
    {
        try
        {
            var action = new TAction();
            action.Execute(); 
        }
        catch (Exception ex)
        {
            Collective.Log.Error($"Failed to execute action {typeof(TAction).Name}: {ex}");
        }
    }
}
