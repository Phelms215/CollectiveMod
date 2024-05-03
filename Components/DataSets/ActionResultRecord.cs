using Collective.Definitions;

namespace Collective.Components.DataSets;
 
public class ActionResultRecord<T> where T : class
{
    public T Data { get; }
    public bool Success { get; }
    public string Message { get; }

    public ActionResultRecord(T data, bool success = true, string message = Key.SuccessAction)
    {
        Data = data;
        Success = success;
        Message = message;
    }
}

public class ActionResultRecord
{
    public bool Status { get; }
    public string Message { get; }

    public ActionResultRecord(bool status = true, string message = Key.SuccessAction)
    {
        Status = status;
        Message = message;
    }
}
