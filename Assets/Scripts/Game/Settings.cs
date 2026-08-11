
[System.Serializable]
public class Settings
{
    public int resolutionWidth;
    public int resolutionHeight;
    public bool isFullscreen;
    public bool useVSync;
    public int textureQuality; // 0: Yüksek (Full), 1: Orta (Half), 2: Düşük (Quarter)
    public float fov;
    public int language;
    public float computerScreenDistance;
    public string bindingOverridesJson;
    
    public Settings Clone()
    {
        // CurrentSettings referansını değil, bağımsız bir kopyasını döndürür.
        return new Settings
        {
            resolutionWidth = resolutionWidth,
            resolutionHeight = resolutionHeight,
            isFullscreen = isFullscreen,
            useVSync = useVSync,
            textureQuality = textureQuality,
            fov = fov,
            language = language,
            computerScreenDistance = computerScreenDistance,
            bindingOverridesJson = bindingOverridesJson
        };
    }
}   