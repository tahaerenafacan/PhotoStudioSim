using System.Collections.Generic;
using UnityEngine;
using Evo.UI;
using NaughtyAttributes;
using SyntaxSultan.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private RectTransform pauseMenu;
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;
    [SerializeField] private AnimatedContainer animContainer;
    
    [SerializeField] private ModalWindow confirmationPopup;
    
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMainMenuButton;
    [SerializeField] private Button quitToDesktopButton;
    private readonly List<ButtonBinder> buttonBinders = new();
    
    [SerializeField] private SettingsUI settingsUI;
    
    [Scene, SerializeField] private int mainMenuScene;
    
    private void Start()
    {
        GameManager.Instance.OnGamePause += GameManager_OnGamePause;
        GameManager.Instance.OnGameResume += GameManager_OnGameResume;
        
        buttonBinders.Add(new ButtonBinder(resumeButton, ResumeGame));
        buttonBinders.Add(new ButtonBinder(settingsButton, OpenSettings));
        buttonBinders.Add(new ButtonBinder(quitToMainMenuButton, ConfirmQuitToMainMenu));
        buttonBinders.Add(new ButtonBinder(quitToDesktopButton, ConfirmQuitToDesktop));
    }
    
    private void OnDestroy()
    {
        foreach (var binder in buttonBinders)
            binder.Unbind();
        
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnGamePause -= GameManager_OnGamePause;
        GameManager.Instance.OnGameResume -= GameManager_OnGameResume;
    }

    private void GameManager_OnGameResume()
    {
        FunctionLibrary.SetCanvasGroupActive(ref pauseMenuCanvasGroup, false);
        settingsUI.CloseSettings();
    }

    private void GameManager_OnGamePause()
    {
        FunctionLibrary.SetCanvasGroupActive(ref pauseMenuCanvasGroup, true);
        animContainer.Animate();
    }

    private void ResumeGame()
    {
        GameManager.Instance.ResumeGame();
    }

    private void OpenSettings()
    {
        settingsUI.OpenSettings();
    }

    private void ConfirmQuitToDesktop()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Quit to Desktop");
        confirmationPopup.SetDescription("Are you sure you want to quit to desktop?\nAny unsaved progress will be lost.");
        confirmationPopup.onConfirm.AddListener((() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
#else
            Application.Quit(); 
#endif
        }));
        confirmationPopup.Open();
    }

    private void ConfirmQuitToMainMenu()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Quit to Main Menu");
        confirmationPopup.SetDescription("Are you sure you want to quit to the main menu?\nAny unsaved progress will be lost.");
        confirmationPopup.onConfirm.AddListener(() => SceneManager.LoadScene(mainMenuScene) );
        confirmationPopup.Open();
    }
}
