using TMPro;
using UniStorm;
using UnityEngine;
using UnityEngine.UI;

public class ComputerSettings : MonoBehaviour
{
    public static ComputerSettings Instance { get; private set; }

    [SerializeField] private Image wallpaperImage;
    [SerializeField] private TextMeshProUGUI timeText;
    
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

    void Start()
    {
        UniStormManager.Instance.OnTimeChange += UpdateTimeDisplay;
    }

    private void UpdateTimeDisplay(int hour, int minute)
    {
        timeText.text = $"{hour:00}:{minute:00}";
    }

    public void SetWallpaper(Sprite wallpaper)
    {
        this.wallpaper = wallpaper;
        wallpaperImage.sprite = wallpaper;
    }
}
