using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicAppWindow : AppWindow
{
    [Header("Music App Window")]
    
    [Header("Playlist")]
    [SerializeField] private Transform playlistContainer;
    [SerializeField] private PlaylistItem playListItemPrefab;
    
    [Header("Record")]
    [SerializeField] private Transform recordImage;
    [SerializeField] private TextMeshProUGUI trackNameText;
    [SerializeField] private float recordRotateSpeed;
    
    [Header("Volume")]
    [SerializeField] private Slider  trackVolumeSlider;
    
    [Header("Playbar")]
    [SerializeField] private Button prevButton;  
    [SerializeField] private Button playButton;
    [SerializeField] private Image playButtonImage;
    [SerializeField] private Sprite playingSprite;
    [SerializeField] private Sprite pausedSprite;
    [SerializeField] private Button nextButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI timestampText; //i.e 2:50/3:10
 
    // ── Runtime ─────────────────────────────────────────────
    private MusicService service;
    private PlaylistItem[] playlistItems;
    
    // ── AppWindow overrides ───────────────────────────────────────
 
    protected override void OnOpened()
    {
        service = MusicService.Instance;
 
        BuildPlaylist();
        BindButtons();
        SyncAllUI();
 
        service.OnTrackChanged     += OnTrackChanged;
        service.OnPlayStateChanged += OnPlayStateChanged;
    }
 
    protected override void OnClosed() => Unsubscribe();
    private void OnDestroy() => Unsubscribe();
 
    private void Unsubscribe()
    {
        if (service == null) return;
        service.OnTrackChanged     -= OnTrackChanged;
        service.OnPlayStateChanged -= OnPlayStateChanged;
        service = null;
    }
 
    private void Update()
    {
        if (service == null) return;
        if (!service.IsPlaying) return;
        
        RotateRecord();
        SyncProgress();
    }
 
    private void BuildPlaylist()
    {
        foreach (Transform child in playlistContainer)
            Destroy(child.gameObject);
 
        var tracks = service.Tracks;
        playlistItems = new PlaylistItem[tracks.Length];
 
        for (int i = 0; i < tracks.Length; i++)
        {
            var item = Instantiate(playListItemPrefab, playlistContainer);
            item.SetTrackName(tracks[i].name);
            item.SetTrackActive(i == service.CurrentIndex);
 
            int captured = i;
            item.SetOnClick(() => service.PlayTrack(captured));
            playlistItems[i] = item;
        }
    }
 
    // ── Button binding ────────────────────────────────────────────
 
    private void BindButtons()
    {
        playButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        prevButton.onClick.RemoveAllListeners();
 
        playButton.onClick.AddListener(service.TogglePlayPause);
        nextButton.onClick.AddListener(service.PlayNext);
        prevButton.onClick.AddListener(service.PlayPrev);
 
        trackVolumeSlider.SetValueWithoutNotify(service.GetVolume());
        trackVolumeSlider.onValueChanged.RemoveAllListeners();
        trackVolumeSlider.onValueChanged.AddListener(service.SetVolume);
 
        progressBar.onValueChanged.RemoveAllListeners();
        progressBar.onValueChanged.AddListener(OnProgressDragged);
    }
 
    // ── Service event handlers ────────────────────────────────────
 
    private void OnTrackChanged(int index)
    {
        trackNameText.text = service.Tracks[index].name;
        RefreshPlaylistHighlight(index);
        RefreshNavButtons();
    }
 
    private void OnPlayStateChanged(bool playing)
    {
        RefreshPlayButton(playing);
    }
 
    // ── Progress seek ─────────────────────────────────────────────
 
    private void OnProgressDragged(float value)
    {
        var clip = service.Source.clip;
        if (clip == null) return;
        service.Source.time = value * clip.length;
        SyncTimestamp();
    }
 
    // ── UI sync ───────────────────────────────────────────────────
 
    private void SyncAllUI()
    {
        bool hasTrack = service.CurrentIndex >= 0;
 
        trackNameText.text = hasTrack ? service.Tracks[service.CurrentIndex].name : "No active track";
 
        RefreshPlaylistHighlight(service.CurrentIndex);
        RefreshPlayButton(service.IsPlaying);
        RefreshNavButtons();
        SyncProgress();
    }
 
    private void RotateRecord()
    {
        recordImage.Rotate(0f, 0f, -recordRotateSpeed * Time.deltaTime);
    }
 
    private void SyncProgress()
    {
        var src = service.Source;
        if (src.clip == null || src.clip.length <= 0f) return;
 
        progressBar.SetValueWithoutNotify(src.time / src.clip.length);
        SyncTimestamp();
    }
 
    private void SyncTimestamp()
    {
        var src = service.Source;
        if (src.clip == null) { timestampText.text = "0:00/0:00"; return; }
        timestampText.text = $"{FormatTime(src.time)}/{FormatTime(src.clip.length)}";
    }
 
    private void RefreshPlayButton(bool playing)
    {
        if (playButtonImage) playButtonImage.sprite = playing ? playingSprite : pausedSprite;
    }
 
    private void RefreshNavButtons()
    {
        prevButton.interactable = service.CurrentIndex > 0;
        nextButton.interactable = service.CurrentIndex < service.Tracks.Length - 1;
    }
 
    private void RefreshPlaylistHighlight(int activeIndex)
    {
        if (playlistItems == null) return;
        for (int i = 0; i < playlistItems.Length; i++)
            playlistItems[i].SetTrackActive(i == activeIndex);
    }
 
    private string FormatTime(float totalSeconds)
    {
        int m = Mathf.FloorToInt(totalSeconds / 60f);
        int s = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{m}:{s:00}";
    }
}
