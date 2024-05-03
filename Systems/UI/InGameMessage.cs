using System;
using Collective.Utilities;
using TMPro;
using UnityEngine;

namespace Collective.Systems.UI;

public class InGameMessage : UIParent
{
    public InGameMessage()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("OnScreenNotification");
    }

    public override void UpdateView()
    {
        
    }

    public void DisplayMessage(string message)
    {
        if (ThisObject == null) return;
        ThisObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
        ThisObject.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
        ThisObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 8;
        ThisObject.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().fontSize = 8;
        ThisObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        ThisObject.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
        ThisObject.SetActive(true);
    }
}