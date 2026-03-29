using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicService : MonoBehaviour
{
    public static MusicService Instance { get; private set; }

    [SerializeField] private AudioClip[] tracks;

    // ── Events ──
    public event Action<int> OnTrackChanged;      // yeni track index
    public event Action<bool> OnPlayStateChanged; // true = çalıyor

    // ── Public state ──────────────────────────────────────────────
    public int CurrentIndex  { get; private set; } = -1;
    public bool IsPlaying    => audioSource.isPlaying;
    public AudioClip[] Tracks => tracks;
    public AudioSource Source => audioSource;

    private AudioSource audioSource;

    // ── Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.loop        = false;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        ComputerState.Instance.OnPowerOut += OnPowerOut;
    }

    private void OnDestroy()
    {
        if (ComputerState.Instance != null)
            ComputerState.Instance.OnPowerOut -= OnPowerOut;
    }

    private void Update()
    {
        // Şarkı bitti mi? → sonrakine geç
        if (!audioSource.isPlaying || audioSource.clip == null) return;

        // isPlaying zaten true iken clip süresi dolduğunda
        // bir sonraki frame'de isPlaying false olur
        // bu yüzden Update'de değil, aşağıdaki LateUpdate'de kontrol ediyoruz
    }

    private void LateUpdate()
    {
        CheckTrackEnd();
    }

    // ── Power ─────────────────────────────────────────────────────

    private void OnPowerOut()
    {
        audioSource.Stop();
        OnPlayStateChanged?.Invoke(false);
    }

    // ── Public API ────────────────────────────────────────────────

    public void PlayTrack(int index)
    {
        if (!IsValidIndex(index)) return;

        CurrentIndex       = index;
        audioSource.clip   = tracks[index];
        audioSource.time   = 0f;
        audioSource.Play();

        OnTrackChanged?.Invoke(index);
        OnPlayStateChanged?.Invoke(true);
    }

    public void TogglePlayPause()
    {
        if (tracks.Length == 0) return;

        if (CurrentIndex < 0) { PlayTrack(0); return; }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            OnPlayStateChanged?.Invoke(false);
        }
        else
        {
            audioSource.UnPause();
            OnPlayStateChanged?.Invoke(true);
        }
    }

    public void PlayNext()
    {
        if (CurrentIndex < tracks.Length - 1)
            PlayTrack(CurrentIndex + 1);
    }

    public void PlayPrev()
    {
        if (CurrentIndex > 0)
            PlayTrack(CurrentIndex - 1);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetVolume() => audioSource.volume;

    // ── Internal ──────────────────────────────────────────────────

    private void CheckTrackEnd()
    {
        if (CurrentIndex < 0 || audioSource.clip == null) return;
        if (audioSource.isPlaying) return;
        // Clip yüklü, çalmıyor ama biz pause da etmedik → bitti
        // Pause durumunu "time < length" ile ayırt ediyoruz
        if (audioSource.time > 0f && audioSource.time < audioSource.clip.length) return;

        if (CurrentIndex < tracks.Length - 1)
            PlayNext();
        else
        {
            // Playlist bitti, başa sar
            CurrentIndex = -1;
            OnPlayStateChanged?.Invoke(false);
        }
    }

    private bool IsValidIndex(int index) => index >= 0 && index < tracks.Length;
}