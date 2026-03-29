using UnityEngine;
using UnityEngine.UI;

public class ComputerSettings : MonoBehaviour
{
    public static ComputerSettings Instance { get; private set; }

    [SerializeField] private Image wallpaperImage;
    
    private Sprite wallpaper;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    public void SetWallpaper(Sprite wallpaper)
    {
        this.wallpaper = wallpaper;
        wallpaperImage.sprite = wallpaper;
    }
}
