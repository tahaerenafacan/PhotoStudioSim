using UnityEngine;
using Evo.UI;

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
    [SerializeField] private SettingsUI settingsUI;
    
    private void Start()
    {
        GameManager.Instance.OnGamePause += GameManager_OnGamePause;
        GameManager.Instance.OnGameResume += GameManager_OnGameResume;
        
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitToMainMenuButton.onClick.AddListener(ConfirmQuitToMainMenu);
        quitToDesktopButton.onClick.AddListener(ConfirmQuitToDesktop);
    }
    
    private void OnDestroy()
    {
        resumeButton.onClick.RemoveListener(ResumeGame);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitToMainMenuButton.onClick.RemoveListener(ConfirmQuitToMainMenu);
        quitToDesktopButton.onClick.RemoveListener(ConfirmQuitToDesktop);
        
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
        confirmationPopup.onConfirm.AddListener(Application.Quit);
        confirmationPopup.Open();
    }

    private void ConfirmQuitToMainMenu()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Quit to Main Menu");
        confirmationPopup.SetDescription("Are you sure you want to quit to the main menu?\nAny unsaved progress will be lost.");
        confirmationPopup.onConfirm.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu") );
        confirmationPopup.Open();
    }
}
