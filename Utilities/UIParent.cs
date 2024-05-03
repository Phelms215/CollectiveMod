using Collective.Components.Interfaces;
using UnityEngine;

namespace Collective.Utilities;

public abstract class UIParent : MonoBehaviour, IView
{
    protected GameObject? ThisObject = null;


    public abstract void UpdateView(); 
    
    public virtual void Show()
    {
        if (ThisObject == null)
        {
            Collective.Log.Error("Trying to show a null object");
            return;
        }

        ThisObject.SetActive(true);
        UpdateView();
    }

    public virtual void Hide()
    {
        if (ThisObject == null)
        {
            Collective.Log.Error("Trying to hide a null object");
            return;
        }

        ThisObject.SetActive(false);
    }

    public new void Kill(float delay = 0)
    {
        Destroy(ThisObject, delay);
        Destroy(gameObject, delay);
    }
}