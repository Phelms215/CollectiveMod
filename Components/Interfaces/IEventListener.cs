namespace Collective.Components.Interfaces;

public interface IEventListener
{
    public void OnEvent<T>(object? sender, T e);

}