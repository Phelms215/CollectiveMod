namespace Collective.Components.Interfaces;

public interface IGameAction<in TArg> where TArg : class
{
    // Default implementation with a return type
    TReturn? Execute<TReturn>(TArg arg) where TReturn : class
    {
        // Log that the method is not implemented
        Collective.Log.Info($"Attempting to execute {GetType().Name} without an implemented method.");
        return default(TReturn);
    }

    // Default implementation without a return type
    void Execute(TArg arg)
    {
        // Log that the method is not implemented
        Collective.Log.Info($"Attempting to execute {GetType().Name} without an implemented method.");
    }
}

public interface IGameAction
{
    // Generic method that allows any class type argument
    void Execute<TArg>(TArg arg) where TArg : class
    {
        // Log that the method is not implemented
        Collective.Log.Info($"Attempting to execute {GetType().Name} without an implemented argument method.");
    }

    // Non-generic execute method
    void Execute()
    {
        // Log that the method is not implemented
        Collective.Log.Info($"Attempting to execute {GetType().Name} without an implemented method.");
    }
}