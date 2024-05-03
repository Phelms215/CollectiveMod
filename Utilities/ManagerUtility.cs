using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Modals;

namespace Collective.Utilities;

public abstract class ManagerUtility
{ 
    protected ManagerUtility() => EventUtility.Subscribe<SaveData>(ModEventType.InitialLoad, LoadInitialData);
    protected abstract void LoadInitialData(object sender, EventData<SaveData> saveData); 

}