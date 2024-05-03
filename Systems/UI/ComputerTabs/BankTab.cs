using UnityEngine;

namespace Collective.Systems.UI.ComputerTabs;

public class BankTab : MonoBehaviour
{
    private GameObject? _splashScreen;
    private GameObject? _market;

    private void Awake()
    {
        
    }

    public void Show()
    {
        if(_splashScreen == null) return;
        _splashScreen.SetActive(true);
    }
    
    public void Hide()
    {
        if(_splashScreen == null) return;
        if (_splashScreen.activeSelf)
            _splashScreen.SetActive(false);
    }
    
}