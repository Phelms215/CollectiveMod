using BepInEx.Logging;

namespace Collective.Utilities;

public class LogUtility
{
    private readonly ManualLogSource _logger;
    
    public LogUtility(ManualLogSource logger)
    {   
        _logger = logger; 
    }
    
    public void Info(string message)
    {
        _logger.LogInfo(message);
    }
    
    public void Warn(string message)
    {
        _logger.LogWarning(message);
    }
    
    public void Error(string message)
    {
        _logger.LogError(message);
    }
    
    public void Debug(string message)
    {
        _logger.LogDebug(message);
    }
}