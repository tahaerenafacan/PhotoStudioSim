using System;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public Settings CurrentSettings { get; private set; }
    
    public static event Action<float> OnFOVChanged;
    public static event Action<float> OnComputerScreenDistanceChanged;
    public static event Action<Settings> OnSettingsLoaded;

    private string filePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        filePath = Path.Combine(Application.persistentDataPath, "settings.json");
        LoadSettings();
    }
    
    public void UpdateSetting(Settings updatedSettings, bool persistToDisk = true)
    {
        CurrentSettings = updatedSettings;
        ApplyAllSettings();

        if (persistToDisk)
            SaveSettings();
    }

    private void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentSettings, true);
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Ayarlar başarıyla dış dosyaya kaydedildi: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ayarlar dosyaya kaydedilirken hata oluştu: {e.Message}");
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                CurrentSettings = JsonUtility.FromJson<Settings>(json);
            }
            else
            {
                Debug.LogWarning("Ayarlar dosyası bulunamadı, varsayılan ayarlar oluşturuluyor...");
                LoadDefaultSettings();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ayarlar dosyası okunurken hata oluştu (JSON bozulmuş olabilir), varsayılana dönülüyor: {e.Message}");
            LoadDefaultSettings();
        }

        ApplyAllSettings();
        OnSettingsLoaded?.Invoke(CurrentSettings);
    }

    public void LoadDefaultSettings()
    {
        CurrentSettings = new Settings
        {
            resolutionWidth = Screen.currentResolution.width,
            resolutionHeight = Screen.currentResolution.height,
            isFullscreen = true,
            useVSync = false,
            textureQuality = 0,
            fov = 60f,
            language = 0,
            computerScreenDistance = 33f,
            bindingOverridesJson = string.Empty
        };
        
        SaveSettings();
    }

    private void ApplyAllSettings()
    {
        if (Screen.width != CurrentSettings.resolutionWidth ||
            Screen.height != CurrentSettings.resolutionHeight ||
            Screen.fullScreen != CurrentSettings.isFullscreen)
        {
            Screen.SetResolution(CurrentSettings.resolutionWidth, CurrentSettings.resolutionHeight, CurrentSettings.isFullscreen);
        }

        if (QualitySettings.globalTextureMipmapLimit != CurrentSettings.textureQuality)
            QualitySettings.globalTextureMipmapLimit = CurrentSettings.textureQuality;

        int targetVSync = CurrentSettings.useVSync ? 1 : 0;
        if (QualitySettings.vSyncCount != targetVSync)
            QualitySettings.vSyncCount = targetVSync;

        OnFOVChanged?.Invoke(CurrentSettings.fov);
        OnComputerScreenDistanceChanged?.Invoke(CurrentSettings.computerScreenDistance);
        ApplyLanguage(CurrentSettings.language);
    }
    
    private void ApplyLanguage(int langIndex)
    {
        if (LocalizationSettings.AvailableLocales != null && 
            langIndex >= 0 && 
            langIndex < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langIndex];
        }
    }
}