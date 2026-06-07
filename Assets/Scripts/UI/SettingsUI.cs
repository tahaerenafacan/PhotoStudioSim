using System.Collections.Generic;
using Evo.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsUI : MonoBehaviour
{
    [Header("Action Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private ModalWindow confirmationPopup;

    [Header("UI Elements")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown fullscreenToggle;
    [SerializeField] private Dropdown vsyncToggle;
    [SerializeField] private Dropdown textureQualityDropdown;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Selector languageDropdown;

    private bool isDirty;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private Settings localSettings;

    private void Start()
    {
        // Performans için CanvasGroup/Canvas bileşeni ile kapatılması önerilir [cite: 11]
        gameObject.SetActive(false); 

        resetButton.onClick.AddListener(OnResetClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        SetupResolutionDropdown();
        SetupUIEventListeners();
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
        
        // Mevcut ayarları manager'dan klonla
        Settings current = SettingsManager.Instance.CurrentSettings;
        localSettings = JsonUtility.FromJson<Settings>(JsonUtility.ToJson(current));

        UpdateUIElements();
        isDirty = false;
    }
    
    public void CloseSettings()
    {
        gameObject.SetActive(false);
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
            Debug.Log(filteredResolutions[index].width + "x" + filteredResolutions[index].height);
            MarkAsDirty();
        });

        fullscreenToggle.onItemSelected.AddListener(val => {
            localSettings.isFullscreen = val == 1;
            MarkAsDirty();
        });

        vsyncToggle.onItemSelected.AddListener(val => {
            localSettings.useVSync = val == 1;
            MarkAsDirty();
        });

        textureQualityDropdown.onItemSelected.AddListener(index => {
            localSettings.textureQuality = index;
            MarkAsDirty();
        });

        fovSlider.onValueChanged.AddListener(val => {
            localSettings.fov = val;
            MarkAsDirty();
        });

        languageDropdown.onSelectionChanged.AddListener(index => {
            ChangeLanguage(index);
        });
    }

    private void UpdateUIElements()
    {
        // UI elemanlarını localSettings verilerine göre eşitle
        fullscreenToggle.SelectItem(localSettings.isFullscreen ? 1 : 0);
        vsyncToggle.SelectItem(localSettings.useVSync ?  1 : 0);
        textureQualityDropdown.SelectItem(localSettings.textureQuality);
        fovSlider.value = localSettings.fov;
        languageDropdown.SetSelection(localSettings.language);

        // Çözünürlük eşleme
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

    private void MarkAsDirty() => isDirty = true;

    private void OnSaveClicked()
    {
        // Geçici ayarları ana sisteme aktar ve JSON olarak kaydet
        Settings globalSettings = SettingsManager.Instance.CurrentSettings;
        globalSettings.resolutionWidth = localSettings.resolutionWidth;
        globalSettings.resolutionHeight = localSettings.resolutionHeight;
        globalSettings.isFullscreen = localSettings.isFullscreen;
        globalSettings.useVSync = localSettings.useVSync;
        globalSettings.textureQuality = localSettings.textureQuality;
        globalSettings.fov = localSettings.fov;
        globalSettings.language = localSettings.language;

        SettingsManager.Instance.SaveSettings();
        isDirty = false;
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
            isDirty = false;
            confirmationPopup.Close(); // Varsayılan pop-up kapatma metodu
        });
    }

    private void OnExitClicked()
    {
        if (isDirty)
        {
            confirmationPopup.onConfirm.RemoveAllListeners();
            confirmationPopup.SetTitle("Unsaved Changes");
            confirmationPopup.SetDescription("You have unsaved changes. Are you sure you want to exit?");
            confirmationPopup.Open();
            confirmationPopup.onConfirm.AddListener(CloseSettings);
        }
        else
        {
            CloseSettings();
        }
    }

    public void ChangeLanguage(int languageIndex)
    {
        localSettings.language = languageIndex;
        MarkAsDirty();
        if (LocalizationSettings.AvailableLocales.Locales.Count > languageIndex)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
        }
    }
}