using System.IO;
using BepInEx;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Utilities;

public static class UIUtility
{
    public static readonly AssetBundle UIAssetBundle =
        AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "collective.bundle"));

    public static GameObject TabsObject()
    {
        return GameObject.Find("---GAME---/Computer/Screen/Management App/Tabs/");
    }
    
    

    public static TMP_FontAsset GetPrimaryFont()
    {
        var gameObject = GameObject.Find("---GAME---/Computer/Screen/Management App/App Title/Text (TMP)")
            .GetComponent<TextMeshProUGUI>();
        if (gameObject == null)
        {
            throw new FileNotFoundException("Could not find primary font");
        }

        return gameObject.font;
    }

    public static void ClearScrollRect(ScrollRect scrollRect)
    {
        foreach (Transform child in scrollRect.content)
            Object.Destroy(child.gameObject);
    }

    public static Sprite GetSprite(string spriteName)
    {
        var sprite = UIAssetBundle.LoadAsset<Sprite>(spriteName);
        if (sprite != null) return sprite;
        Collective.Log.Error($"Could not find sprite {spriteName}");
        throw new FileNotFoundException($"Could not find sprite {spriteName}");
    }

    public static T? LoadAsset<T>(string prefabName, Transform? parent = null) where T : UnityEngine.Object
    {
        var prefab = UIUtility.UIAssetBundle.LoadAsset<T>(prefabName);
        if (prefab == null)
        {
            Collective.Log.Error("Failed to load asset " + prefabName);
            return null;
        }

        if (parent != null) return Object.Instantiate(prefab, parent) as T;
        return Object.Instantiate(prefab) as T;
    }

    public static int GetStoreLevel() => Singleton<StoreLevelManager>.Instance.CurrentLevel;

}