using Collective.Components.DataSets;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using MyBox;
using Collective.Systems.Managers;

namespace Collective.Systems.Actions;

public class InitializeSaveData : IGameAction<SaveData>
{

    public SaveData Execute()
    {
        Collective.Log.Info("Initializing Save Data");
        Singleton<MoneyManager>.Instance.MoneyTransition(100000, MoneyManager.TransitionType.LOAN_INCOME);
        return new SaveData();
    }

}