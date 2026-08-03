using System.Collections.Generic;
using DG.Tweening;
using Evo.UI;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Animation")] 
    [SerializeField] private float closedPos;
    [SerializeField] private float openPos;
    [SerializeField] private float openTime;
    private Tween onOffTween;
    
    [Header("Action Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private ModalWindow confirmationPopup;

    [Header("UI Elements")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private UnityEngine.UI.Toggle fullscreenToggle;
    [SerializeField] private UnityEngine.UI.Toggle vsyncToggle;
    [SerializeField] private Dropdown textureQualityDropdown;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Selector languageDropdown;
    [SerializeField] private Slider computerScreenDistanceSlider;

    private List<Resolution> filteredResolutions = new List<Resolution>();
    private Settings localSettings;

    private void Start()
    {
        FunctionLibrary.SetCanvasGroupActive(ref canvasGroup, false);
        settingsPanel.anchoredPosition = new Vector2(closedPos, 0);
        
        localSettings = JsonUtility.FromJson<Settings>(JsonUtility.ToJson(SettingsManager.Instance.CurrentSettings));

        resetButton.onClick.AddListener(OnResetClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        SetupResolutionDropdown();
        SetupUIEventListeners();
    }

    public void OpenSettings()
    {
        Debug.Log("Settings Opened");
        FunctionLibrary.SetCanvasGroupActive(ref canvasGroup, true);

        localSettings = SettingsManager.Instance.CurrentSettings.Clone();

        AnimatePanel(openPos, Ease.OutBack, (() => print("End")));
        UpdateUIElements();
    }
    
    public void CloseSettings()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        AnimatePanel(closedPos, Ease.InBack, () =>
        {
            FunctionLibrary.SetCanvasGroupActive(ref canvasGroup, false);
        });
    }
    
    private void AnimatePanel(float targetPosX, Ease ease, TweenCallback onComplete = null)
    {
        onOffTween?.Kill();

        onOffTween = settingsPanel
            .DOAnchorPosX(targetPosX, openTime)
            .SetEase(ease)
            .SetUpdate(true)
            .OnComplete(onComplete);
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearAllItems();
        filteredResolutions = new List<Resolution>();
        List<Dropdown.Item> options = new List<Dropdown.Item>();

        Resolution[] allResolutions = Screen.resolutions;
        double currentRefreshRate = Screen.currentResolution.refreshRateRatio.value;

        // Monitör yenileme hızından ötürü çift kayıt oluşmasını engelle 
        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (Mathf.Approximately((float)allResolutions[i].refreshRateRatio.value, (float)currentRefreshRate))
            {
                filteredResolutions.Add(allResolutions[i]);
                options.Add(new Dropdown.Item($"{allResolutions[i].width} x {allResolutions[i].height}"));
            }
        }

        resolutionDropdown.AddItems(options.ToArray());
    }

    private void SetupUIEventListeners()
    {
        resolutionDropdown.onItemSelected.AddListener(index => {
            localSettings.resolutionWidth = filteredResolutions[index].width;
            localSettings.resolutionHeight = filteredResolutions[index].height;
            ApplySettings();
        });

        fullscreenToggle.onValueChanged.AddListener(val => {
            localSettings.isFullscreen = val;
            ApplySettings();
        });

        vsyncToggle.onValueChanged.AddListener(val => {
            localSettings.useVSync = val;
            ApplySettings();
        });

        textureQualityDropdown.onItemSelected.AddListener(index => {
            localSettings.textureQuality = index;
            ApplySettings();
        });

        fovSlider.onValueChanged.AddListener(val => {
            localSettings.fov = val;
            ApplySettings();
        });
        
        languageDropdown.onSelectionChanged.AddListener(index => {
            localSettings.language = index;
            ApplySettings();
        });

        computerScreenDistanceSlider.onValueChanged.AddListener(val =>
        {
            localSettings.computerScreenDistance = val;
            ApplySettings();
        });
    }
    
    private void ApplySettings()
    {
        SettingsManager.Instance.UpdateSetting(localSettings);
    }

    private void UpdateUIElements()
    {
        fullscreenToggle.isOn = localSettings.isFullscreen;
        vsyncToggle.isOn = localSettings.useVSync;
        textureQualityDropdown.SelectItem(localSettings.textureQuality);
        fovSlider.value = localSettings.fov;
        languageDropdown.SetSelection(localSettings.language);

        int currentResIndex = 0;
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            if (filteredResolutions[i].width == localSettings.resolutionWidth &&
                filteredResolutions[i].height == localSettings.resolutionHeight)
            {
                currentResIndex = i;
                break;
            }
        }
        resolutionDropdown.selectedIndex = currentResIndex;
    }
    

    private void OnResetClicked()
    {
        confirmationPopup.onConfirm.RemoveAllListeners();
        confirmationPopup.SetTitle("Reset Settings");
        confirmationPopup.SetDescription("Are you sure you want to reset all settings to their default values?");
        confirmationPopup.Open();
        confirmationPopup.onConfirm.AddListener(() => {
            SettingsManager.Instance.LoadDefaultSettings();
            localSettings = JsonUtility.FromJson<Settings>(JsonUtility.ToJson(SettingsManager.Instance.CurrentSettings));
            UpdateUIElements();
            confirmationPopup.Close();
        });
    }

    private void OnExitClicked()
    {
        CloseSettings();
    }
    
    private void OnDestroy()
    {
        onOffTween?.Kill();
    }
}