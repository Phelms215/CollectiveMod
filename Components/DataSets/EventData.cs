using Collective.Components.Definitions;

namespace Collective.Components.DataSets;

public class EventData<T> where T : class
{
    public T Data { get; }
    public ModEventType Type { get; }

    public EventData(T data, ModEventType type)
    {
        Data = data;
        Type = type;
    }
}