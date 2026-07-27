using UnityEngine;
using UnityEngine.UI;

public class ComputerSettings : MonoBehaviour
{
    [SerializeField] private Image wallpaperImage;
    
    private Sprite currentWallpaper;

    public void SetWallpaper(Sprite wallpaper)
    {
        currentWallpaper = wallpaper;
        wallpaperImage.sprite = wallpaper;
    }
}
