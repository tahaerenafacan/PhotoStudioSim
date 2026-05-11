using System;
using Evo.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private ModalWindow confirmationPopup;

    private bool isDirty;

    private void Start()
    {
        gameObject.SetActive(false);
        resetButton.onClick.AddListener(OnResetClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnExitClicked()
    {
        if (isDirty)
        {
            confirmationPopup.onConfirm.RemoveAllListeners();
            confirmationPopup.SetTitle("Unsaved Changes");
            confirmationPopup.SetDescription("You have unsaved changes. Are you sure you want to exit?");
            confirmationPopup.Open();
            confirmationPopup.onConfirm.AddListener(() => GameManager.Instance.CloseSettings());
        }
        else
        {
            GameManager.Instance.CloseSettings();
        }
    }

    private void OnSaveClicked()
    {
        isDirty = false;
    }

    private void OnResetClicked()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Reset Settings");
        confirmationPopup.SetDescription("Are you sure you want to reset all settings to their default values?");
        confirmationPopup.Open();
        confirmationPopup.onConfirm.AddListener(() => throw new NotImplementedException());
    }

    public void ChangeLanguage(int languageIndex)
    {
        isDirty = true;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
    }
}
