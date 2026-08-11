using System.Collections.Generic;
using DG.Tweening;
using Evo.UI;
using MoreMountains.Tools;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Rebind UI")]
    [SerializeField] private RectTransform bindingsContainer;
    [SerializeField] private GameObject bindingRowPrefab;
    [SerializeField] private List<RebindActionEntry> rebindEntries;

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

    [NaughtyAttributes.BoxGroup] 
    [SerializeField] private RadialSlider masterAudioSlider;
    [SerializeField] private RadialSlider uiAudioSlider;
    [SerializeField] private RadialSlider sfxAudioSlider;

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
        FunctionLibrary.SetCanvasGroupActive(ref canvasGroup, true);

        localSettings = SettingsManager.Instance.CurrentSettings.Clone();

        AnimatePanel(openPos, Ease.OutBack);
        UpdateUIElements();
        RefreshBindingRows();
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

    public void ToggleSettings()
    {
        if (canvasGroup.alpha > 0.1f)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
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
        
        masterAudioSlider.onValueChanged.AddListener(val =>
        {
            MMSoundManager.Instance.SetVolumeMaster(val/100f);
        });
        uiAudioSlider.onValueChanged.AddListener(val =>
        {
            MMSoundManager.Instance.SetVolumeUI(val/100f);
        });
        sfxAudioSlider.onValueChanged.AddListener(val =>
        {
            MMSoundManager.Instance.SetVolumeSfx(val/100f);
        });
    }

    private void RefreshBindingRows()
    {
        if (rebindEntries == null || rebindEntries.Count == 0) return;

        for (int i = 0; i < rebindEntries.Count; i++)
        {
            var entry = rebindEntries[i];
            if (entry.actionReference == null || entry.row == null) continue;

            string actionName = entry.ActionName;
            string displayString = actionName;
            string bindingText = InputManager.Instance.GetBindingDisplayString(actionName, entry.bindingIndex) ?? string.Empty;
            string effectivePath = InputManager.Instance.GetBindingEffectivePath(actionName, entry.bindingIndex);
            var iconSprite = InputIconResolver.Instance?.GetControlIcon(effectivePath);
            entry.row.Setup(actionName, displayString, bindingText, iconSprite, entry.bindingIndex, RequestRebind);
        }
    }

    private void RequestRebind(string actionName, int bindingIndex)
    {
        InputManager.Instance?.StartInteractiveRebind(actionName, bindingIndex, success =>
        {
            RefreshBindingRows();
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
        resolutionDropdown.selectedIndex = GetSelectedResolutionIndex();
        masterAudioSlider.Value = GetNormalizedTrackValue(MMSoundManager.MMSoundManagerTracks.Master);
        uiAudioSlider.Value     = GetNormalizedTrackValue(MMSoundManager.MMSoundManagerTracks.UI);
        sfxAudioSlider.Value    = GetNormalizedTrackValue(MMSoundManager.MMSoundManagerTracks.Sfx);
    }

    private float GetNormalizedTrackValue(MMSoundManager.MMSoundManagerTracks track)
    {
        return Mathf.RoundToInt(MMSoundManager.Instance.GetTrackVolume(track, false) * 100f);
    }

    private int GetSelectedResolutionIndex()
    {
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
        return currentResIndex;
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
            MMSoundManager.Instance.ResetSettings();
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