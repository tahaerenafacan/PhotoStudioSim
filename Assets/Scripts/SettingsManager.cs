using System;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public Settings CurrentSettings { get; private set; }
    
    public static event Action<float> OnFOVChanged;

    private string filePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        filePath = Path.Combine(Application.persistentDataPath, "settings.json");
        LoadSettings();
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentSettings, true);
            
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Ayarlar başarıyla dış dosyaya kaydedildi: {filePath}");
            ApplyAllSettings();
        }
        catch (Exception e)
        {
            Debug.LogError($"Ayarlar dosyaya kaydedilirken hata oluştu: {e.Message}");
        }
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                CurrentSettings = JsonUtility.FromJson<Settings>(json);
                Debug.Log($"Ayarlar dış dosyadan başarıyla yüklendi: {filePath}");
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
            language = 0
        };
        
        SaveSettings();
    }

    public void ApplyAllSettings()
    {
        Screen.SetResolution(CurrentSettings.resolutionWidth, CurrentSettings.resolutionHeight, CurrentSettings.isFullscreen);
        QualitySettings.globalTextureMipmapLimit = CurrentSettings.textureQuality;
        QualitySettings.vSyncCount = CurrentSettings.useVSync ? 1 : 0;
        
        OnFOVChanged?.Invoke(CurrentSettings.fov);
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