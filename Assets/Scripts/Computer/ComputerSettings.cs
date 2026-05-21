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
        UniStormManager.Instance.OnMinuteChanged += UpdateTimeDisplay;
    }

    private void UpdateTimeDisplay()
    {
        timeText.text = $"{UniStormManager.Instance.GetHour()}:{UniStormManager.Instance.GetMinutes()}";
    }

    public void SetWallpaper(Sprite wallpaper)
    {
        this.wallpaper = wallpaper;
        wallpaperImage.sprite = wallpaper;
    }
}
