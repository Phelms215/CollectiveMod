using System;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Managers;
using Collective.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Collective.Systems.UI.ComputerTabs;

public class SettingsTab : UIParent
{

    private SaveData.MiscSettings? _miscSettings;

    public void Awake()
    {
        LoadShopSettingsButton();
        LoadShopSettingsPanel();
        _miscSettings = Collective.GetManager<GameDataManager>().GetSaveData().Settings;
        UpdateView();
    }

    private void OnEnable() => UpdateView();


    private void LoadShopSettingsButton()
    {
        var baseGameObject = GameObject.Find("---GAME---");
        var taskbar = baseGameObject.transform.GetChild(2).transform.GetChild(2).transform.GetChild(2).transform
            .GetChild(2).gameObject;
        taskbar.transform.GetChild(1).gameObject.SetActive(false);

        var bankButton = GameObject.Find("---GAME---/Computer/Screen/Management App/Taskbar/Buttons/Bank Tab Button");
        bankButton.GetComponentInChildren<TextMeshProUGUI>().text = "Settings";
        bankButton.SetActive(true);
        var image = bankButton.transform.GetChild(1).GetComponentInChildren<Image>();
        var icon = GameObject.Find("---GAME---/Computer/Screen/Management App/App Title/Icon");
        image.sprite = icon.GetComponent<Image>().sprite;
    }

    private void LoadShopSettingsPanel()
    {
        ThisObject = UIUtility.LoadAsset<GameObject>("SettingsPanel", UIUtility.TabsObject().transform);

        if (ThisObject == null) return;
        ThisObject.transform.localPosition = UIUtility.TabsObject().transform.GetChild(1).localPosition;
        var font = UIUtility.GetPrimaryFont();

        ThisObject.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().font = font;
        ThisObject.transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>().font = font;
        UpdateView();
        StoreHourFormButtonListeners();
        ThisObject.SetActive(false);
    }

    public override void UpdateView()
    {
        if (ThisObject == null) return;
        if (_miscSettings == null) return;
        _miscSettings = Collective.GetManager<GameDataManager>().GetSaveData().Settings;

        var restrictions = Collective.GetManager<PermitManager>().OperatingRestrictions();
        if (restrictions == null)
        {
            ThisObject.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "24/7";
            ThisObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "24/7";
        }
        else
        {
            ThisObject.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text =
                restrictions.Open.Hour.ToString("00") + ":" + restrictions.Open.Minute.ToString("00");
            ThisObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text =
                restrictions.Close.Hour.ToString("00") + ":" + restrictions.Close.Minute.ToString("00");
        }

        var storeHours = _miscSettings.StoreHours;

        var openStoreClockFrame = ThisObject.transform.GetChild(1).transform.GetChild(1);
        openStoreClockFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            storeHours.Open.Hour.ToString("00");
        openStoreClockFrame.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            storeHours.Open.Minute.ToString("00");

        var closeStoreClockFrame = ThisObject.transform.GetChild(2).transform.GetChild(1);
        closeStoreClockFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            storeHours.Close.Hour.ToString("00");
        closeStoreClockFrame.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            storeHours.Close.Minute.ToString("00");
    }

    private void AutoOpenCloseToggled(bool isChecked)
    {
        var gameDataManager = Collective.GetManager<GameDataManager>();
        gameDataManager.UpdateAutoOpenClose(isChecked);
    }

    private void StoreHourFormButtonListeners()
    {
        if (ThisObject == null) return;

        // AutoToggle 
        var autoOpen = ThisObject.transform.GetChild(3).GetComponent<Toggle>();
        autoOpen.SetIsOnWithoutNotify(Collective.GetManager<GameDataManager>().GetAutoOpenSetting());
        autoOpen.onValueChanged.AddListener(AutoOpenCloseToggled);


        // Clock Hour For Open Store Panel
        var openStoreButtons = ThisObject.transform.GetChild(1).GetComponentsInChildren<Button>();
        foreach (var button in openStoreButtons)
        {
            switch (button.name)
            {
                case "HourUpButton":
                    button.onClick.AddListener(() => OpenStoreButtonPressed(TimeButtons.HourUpButton));
                    break;
                case "HourDownButton":
                    button.onClick.AddListener(() => OpenStoreButtonPressed(TimeButtons.HourDownButton));
                    break;
                case "MinuteUpButton":
                    button.onClick.AddListener(() => OpenStoreButtonPressed(TimeButtons.MinuteUpButton));
                    break;
                case "MinuteDownButton":
                    button.onClick.AddListener(() => OpenStoreButtonPressed(TimeButtons.MinuteDownButton));
                    break;
            }
        }

        // Clock Hour For Close Store Panel
        var closeStoreButtons = ThisObject.transform.GetChild(2).GetComponentsInChildren<Button>();
        foreach (var button in closeStoreButtons)
        {
            switch (button.name)
            {
                case "HourUpButton":
                    button.onClick.AddListener(() => CloseStoreButtonPressed(TimeButtons.HourUpButton));
                    break;
                case "HourDownButton":
                    button.onClick.AddListener(() => CloseStoreButtonPressed(TimeButtons.HourDownButton));
                    break;
                case "MinuteUpButton":
                    button.onClick.AddListener(() => CloseStoreButtonPressed(TimeButtons.MinuteUpButton));
                    break;
                case "MinuteDownButton":
                    button.onClick.AddListener(() => CloseStoreButtonPressed(TimeButtons.MinuteDownButton));
                    break;
            }
        }
    }


    private void OpenStoreButtonPressed(TimeButtons buttonType)
    {
        if (_miscSettings == null) return;
        var storeHours = _miscSettings.StoreHours;
        StoreHours newStoreHours;
        switch (buttonType)
        {
            case TimeButtons.HourUpButton:
                if (storeHours.Open.Hour == 23)
                    newStoreHours = new StoreHours(0, storeHours.Open.Minute, storeHours.Close.Hour,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour + 1, storeHours.Open.Minute,
                        storeHours.Close.Hour, storeHours.Close.Minute);
                break;
            case TimeButtons.HourDownButton:
                if (storeHours.Open.Hour == 0)
                    newStoreHours = new StoreHours(23, storeHours.Open.Minute,
                        storeHours.Close.Hour,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour - 1, storeHours.Open.Minute,
                        storeHours.Close.Hour, storeHours.Close.Minute);
                break;
            case TimeButtons.MinuteUpButton:
                if (storeHours.Open.Minute == 59)
                    newStoreHours = new StoreHours(storeHours.Open.Hour, 0,
                        storeHours.Close.Hour,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour, storeHours.Open.Minute + 1,
                        storeHours.Close.Hour, storeHours.Close.Minute);
                break;
            case TimeButtons.MinuteDownButton:
                if (storeHours.Open.Minute == 0)
                    newStoreHours = new StoreHours(storeHours.Open.Hour, 59, storeHours.Close.Hour,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour, storeHours.Open.Minute - 1,
                        storeHours.Close.Hour, storeHours.Close.Minute);
                break;
            default:
                return;
        }

        if (!Collective.GetManager<PermitManager>().ValidateStoreHours(newStoreHours)) return;
        Collective.GetManager<GameDataManager>().UpdateStoreHours(newStoreHours);
        UpdateView();
    }

    private void CloseStoreButtonPressed(TimeButtons buttonType)
    {
        if (_miscSettings == null) return;
        var storeHours = _miscSettings.StoreHours;
        StoreHours newStoreHours;
        switch (buttonType)
        {
            case TimeButtons.HourUpButton:
                if (storeHours.Close.Hour == 23)
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute, 0,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute,
                        storeHours.Close.Hour + 1, storeHours.Close.Minute);
                break;
            case TimeButtons.HourDownButton:
                if (storeHours.Close.Hour == 0)
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute, 23,
                        storeHours.Close.Minute);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute,
                        storeHours.Close.Hour - 1, storeHours.Close.Minute);
                break;
            case TimeButtons.MinuteUpButton:
                if (storeHours.Close.Minute == 59)
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute, storeHours.Close.Hour, 0);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute,
                        storeHours.Close.Hour, storeHours.Close.Minute + 1);
                break;
            case TimeButtons.MinuteDownButton:
                if (storeHours.Close.Minute == 0)
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute,
                        storeHours.Close.Hour, 59);
                else
                    newStoreHours = new StoreHours(storeHours.Open.Hour,
                        storeHours.Open.Minute,
                        storeHours.Close.Hour, storeHours.Close.Minute - 1);
                break;
            default:
                return;
        }

        if (!Collective.GetManager<PermitManager>().ValidateStoreHours(newStoreHours)) return;
        Collective.GetManager<GameDataManager>().UpdateStoreHours(newStoreHours);
        UpdateView();
    }
}