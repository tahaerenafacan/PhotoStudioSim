using System;
using UnityEngine;
using UnityEngine.UI;

public class WallpaperPreviewBox : MonoBehaviour
{
    [SerializeField] private Image selectedImage;
    [SerializeField] private Image wallpaperImage;
    [SerializeField] private Button selectButton;
    
    public event Action OnWallpaperClicked;

    private void Awake()
    {
        selectButton.onClick.AddListener(OnClicked);
        SetSelected(false);
    }

    private void OnClicked()
    {
        OnWallpaperClicked?.Invoke();
    }
    
    public void SetSelected(bool selected)
    {
        selectedImage.gameObject.SetActive(selected);
    }

    public void SetWallpaperImage(Sprite wallpaper)
    {
        wallpaperImage.sprite = wallpaper;
    }
}
