
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
}