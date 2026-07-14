using UnityEngine;
using UnityEngine.UI;

public class ComputerSettings : MonoBehaviour
{
    [SerializeField] private Image wallpaperImage;
    
    private Sprite wallpaper;

    public void SetWallpaper(Sprite wallpaper)
    {
        this.wallpaper = wallpaper;
        wallpaperImage.sprite = wallpaper;
    }
}
