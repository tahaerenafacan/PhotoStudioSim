using UnityEngine;

public class SettingsAppWindow : AppWindow
{
    [SerializeField] private Transform wallpapersContainer;
    [SerializeField] private WallpaperPreviewBox wallpaperPreviewBoxPrefab;
    [SerializeField] private Sprite[] wallpapers;
    
    private WallpaperPreviewBox selectedBox;


    protected override void OnOpened()
    {
        selectedBox = null;
        FunctionLibrary.DestroyChildren(wallpapersContainer);

        foreach (var wallpaper in wallpapers)
        {
            WallpaperPreviewBox box = Instantiate(wallpaperPreviewBoxPrefab, wallpapersContainer);
            box.SetWallpaperImage(wallpaper);

            box.OnWallpaperClicked += () => HandleWallpaperSelected(box, wallpaper);
        }
    }

    private void HandleWallpaperSelected(WallpaperPreviewBox box, Sprite wallpaper)
    {
        if (selectedBox != null)
            selectedBox.SetSelected(false);

        selectedBox = box;
        selectedBox.SetSelected(true);

        ComputerSettings.Instance.SetWallpaper(wallpaper);
    }
}
