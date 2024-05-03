using UnityEngine;

namespace Collective.Components.Interfaces;

public interface IOverlayForm<in TArg> where TArg : class
{
 
    public void Load(TArg argument, GameObject formObject);


    // Submit Button 
    // Return true and the form will be closed, false and the form will remain open
    public bool SubmitButtonHandler() => true;

    // Cancel Button 
    // Return true and the form will be closed, false and the form will remain open
    public bool CancelButtonHandler() => true;


}