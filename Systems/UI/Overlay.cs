using System;
using System.Collections.Generic; 
using Collective.Components.Interfaces;
using Collective.Systems.Managers;
using Collective.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Systems.UI;

public class Overlay : UIParent
{

    private Button _closeButton;
    private Button _submitButton;
    private TextMeshProUGUI _headerText;
    private TextMeshProUGUI _bodyText;
    private GameObject _bodyForm; 

    public void Awake()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("PopUpWindow");
        if (ThisObject == null) throw new NullReferenceException("Could not find overlay");
        Hide();

        _closeButton = ThisObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(2).transform.GetChild(1).GetComponent<Button>();
        _submitButton = ThisObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(2).transform.GetChild(0).GetComponent<Button>();
        _headerText = ThisObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _bodyText = ThisObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _bodyForm = ThisObject.transform.GetChild(0).transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).gameObject;
    }

    public override void UpdateView()
    {
        
    }
    
    public void ShowMessage(string title, string message)
    {
        _headerText.text = title;
        _bodyText.text = message;
        _closeButton.gameObject.SetActive(false);
        _submitButton.onClick.AddListener(() => Collective.GetManager<UIManager>().HideOverlay());
    }

    public void Confirmation(string title, string message,
        Func<bool>? cancelAction = null,
        Func<bool>? submitAction = null)
    {
        _headerText.text = title;
        _bodyText.text = message;
        _closeButton.onClick.RemoveAllListeners();
        _submitButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(() =>
        {
            if (cancelAction != null)
            {
                // Only destroy the overlay if the cancel action returns true
                var result = cancelAction();
                if (!result) return; 
            }
            Collective.GetManager<UIManager>().HideOverlay();
        });
        
        _submitButton.onClick.AddListener(() =>
        {
            if (submitAction != null)
            {  
                // Only destroy the overlay if the cancel action returns true
                var result = submitAction();
                if (!result) return; 
            }
            Collective.GetManager<UIManager>().HideOverlay();
        });
        Show();
    }

    public void RegisterButtonListeners<TCloseAction, TSubmitAction>()
        where TCloseAction : IGameAction, new() 
        where TSubmitAction : IGameAction, new()
    {
        _closeButton.onClick.RemoveAllListeners();
        _submitButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(() => ActionUtility.Run<TCloseAction>());
        _submitButton.onClick.AddListener(() => ActionUtility.Run<TSubmitAction>());
    }
    
    public void HideCancelButton() => _closeButton.gameObject.SetActive(false);
    

    public void LoadForm<TForm, TFormArg>(string formName, string formTitle, TFormArg arg)
        where TForm : IOverlayForm<TFormArg>, new()
        where TFormArg : class
    {
        if (ThisObject == null) throw new NullReferenceException("Could not find overlay"); 

        var size = ThisObject.transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>().sizeDelta;
        _bodyText.gameObject.SetActive(false);
        
        var thisForm = UIUtility.LoadAsset<GameObject>(formName, _bodyForm.transform);
        if (thisForm == null) throw new NullReferenceException("Could not find overlay form " + formName);

        ThisObject.transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>().sizeDelta =
            new Vector2(size.x, thisForm.GetComponent<RectTransform>().sizeDelta.y + 75);

        var form = new TForm();
        form.Load(arg, thisForm);
        
        _headerText.text = formTitle;

        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(() =>
        {
            var result = form.CancelButtonHandler();
            if (!result) return;
            Collective.GetManager<UIManager>().HideOverlay();
        });
         
        _submitButton.onClick.RemoveAllListeners();
        _submitButton.onClick.AddListener(() =>
        {
            var result = form.SubmitButtonHandler();
            if (!result) return;
            Collective.GetManager<UIManager>().HideOverlay();
        });

        Show();
    }


    public void SetHeader(string header)
    {
        _headerText.text = header;
    }
    
    public void SetBody(string body)
    {
        _bodyText.text = body;
    }

    public void SetCloseButtonText(string text)
    {
        _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    
    public void SetSubmitButtonText(string text)
    {
        _submitButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    
    
 
}