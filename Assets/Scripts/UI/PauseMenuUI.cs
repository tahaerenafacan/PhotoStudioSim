using UnityEngine;
using Evo.UI;
using System;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private ModalWindow confirmationPopup;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMainMenuButton;
    [SerializeField] private Button quitToDesktopButton;

    private void OnEnable()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitToMainMenuButton.onClick.AddListener(ConfirmQuitToMainMenu);
        quitToDesktopButton.onClick.AddListener(ConfirmQuitToDesktop);
    }

    private void OnDisable()
    {
        resumeButton.onClick.RemoveListener(ResumeGame);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitToMainMenuButton.onClick.RemoveListener(ConfirmQuitToMainMenu);
        quitToDesktopButton.onClick.RemoveListener(ConfirmQuitToDesktop);
    }

    private void ResumeGame()
    {
        GameManager.Instance.TogglePauseGame();
    }

    private void OpenSettings()
    {
        GameManager.Instance.OpenSettings();
    }

    private void ConfirmQuitToDesktop()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Quit to Desktop");
        confirmationPopup.SetDescription("Are you sure you want to quit to desktop?\nAny unsaved progress will be lost.");
        confirmationPopup.onConfirm.AddListener(() => Application.Quit());
        confirmationPopup.Open();
    }

    private void ConfirmQuitToMainMenu()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Quit to Main Menu");
        confirmationPopup.SetDescription("Are you sure you want to quit to the main menu?\nAny unsaved progress will be lost.");
        confirmationPopup.onConfirm.AddListener(() => throw new NotImplementedException());
        confirmationPopup.Open();
    }
}
