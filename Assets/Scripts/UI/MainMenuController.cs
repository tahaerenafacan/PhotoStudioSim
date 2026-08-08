using System.Collections.Generic;
using Evo.UI;
using MoreMountains.Feedbacks;
using SyntaxSultan.UI;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button resumeGameButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private ModalWindow modalWindow;
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private MMF_Player sceneLoader;
    
    private readonly List<ButtonBinder> buttonBinders =  new();

    private void Awake()
    {
        buttonBinders.Add(new ButtonBinder(resumeGameButton, OnResumeClicked));
        buttonBinders.Add(new ButtonBinder(newGameButton, OnNewGameClicked));
        buttonBinders.Add(new ButtonBinder(settingsButton, OnSettingsClicked));
        buttonBinders.Add(new ButtonBinder(quitButton, OnQuitClicked));
    }
    
    private void OnDestroy()
    {
        foreach (var binder in buttonBinders)
            binder.Unbind();
    }

    private void OnResumeClicked()
    {
        sceneLoader.PlayFeedbacks();
    }

    public void OnNewGameClicked()
    {
        Debug.Log("OnNewGameClicked");
    }
    
    private void OnSettingsClicked()
    {
        settingsUI.ToggleSettings();
    }
    
    private void OnQuitClicked()
    {
        modalWindow.onConfirm.RemoveAllListeners();
        modalWindow.SetTitle("Quit to Desktop");
        modalWindow.SetDescription("Are you sure you want to quit the desktop?");
        modalWindow.onConfirm.AddListener(Application.Quit);
        modalWindow.Open();
    }
}