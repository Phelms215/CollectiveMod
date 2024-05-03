using UnityEngine;

namespace Collective.Components.Interfaces;

public interface IView
{
    public void Show();
    public void Hide();
    
    public void UpdateView();
    
    public void Kill(float delay = 0);
    
}