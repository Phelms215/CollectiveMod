using Collective.Systems.Managers;
using HarmonyLib;

namespace Collective.Patches;
 
[HarmonyPatch(typeof (ES3), "StoreCachedFile", new[] { typeof(string) })]
public class SaveGamePatch
{

    [HarmonyPostfix]
    public static void Postfix(string filePath)
    {
        Collective.GetManager<GameDataManager>().Save(filePath);
    }
    
}