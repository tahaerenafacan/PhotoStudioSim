using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("Gerçek hayatta 1 günün kaç saniye süreceği (Örn: 1440 sn = 24 dakika)")]
    [SerializeField] private float dayDurationInRealSeconds = 1440f; 
    [Tooltip("Oyun başladığında saat kaç olacak? (0-24)")]
    [SerializeField, Range(0f, 24f)] private float initialHour = 6f;

    [Header("Day & Night Settings")]
    [SerializeField, Range(0f, 24f)] private float sunriseHour = 6f; // Gündoğumu
    [SerializeField, Range(0f, 24f)] private float sunsetHour = 18f; // Günbatımı
    [SerializeField] private float transitionDuration = 1f; // Güneşin doğuş/batış geçiş süresi (oyun içi saat olarak)
    [SerializeField] private float nightSunIntensity = 0.1f; // Gece güneş/ay ışığı gücü

    // Public Properties
    public int CurrentDay { get; private set; } = 1;
    public float CurrentTimeOfDay { get; private set; } // 0 ile 24 arası sürekli akan zaman
    public float DayProgress => CurrentTimeOfDay / 24f; // 0 ile 1 arası 
    public bool IsDay { get; private set; } = true;
    public float SunIntensity { get; private set; } = 1f;

    public event Action<int> OnDayChanged;
    public event Action<int, int> OnTimeChanged;

    private int lastBroadcastedMinute = -1;
    private float timeMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentTimeOfDay = initialHour;
        
        // 1 gerçek saniyede kaç oyun saati geçeceğini hesapla
        timeMultiplier = 24f / dayDurationInRealSeconds; 
    }

    private void Update()
    {
        UpdateTime();
        UpdateDayNightCycle();
        CheckTimeChangedEvent();
        HandleDebugInput();
    }

    private void UpdateTime()
    {
        CurrentTimeOfDay += Time.deltaTime * timeMultiplier;

        if (CurrentTimeOfDay >= 24f)
        {
            CurrentTimeOfDay %= 24f; // 24'ü geçerse sıfırla ama küsuratı koru (örn: 24.1 -> 0.1)
            CurrentDay++;
            OnDayChanged?.Invoke(CurrentDay);
        }
    }

    private void UpdateDayNightCycle()
    {
        IsDay = CurrentTimeOfDay >= sunriseHour && CurrentTimeOfDay < sunsetHour;

        // Güneş ışığı yoğunluğunu yumuşak (Lerp) bir şekilde hesapla
        if (CurrentTimeOfDay >= sunriseHour && CurrentTimeOfDay < sunriseHour + transitionDuration)
        {
            // Gündoğumu geçişi
            float t = (CurrentTimeOfDay - sunriseHour) / transitionDuration;
            SunIntensity = Mathf.Lerp(nightSunIntensity, 1f, t);
        }
        else if (CurrentTimeOfDay >= sunsetHour - transitionDuration && CurrentTimeOfDay < sunsetHour)
        {
            // Günbatımı geçişi
            float t = (CurrentTimeOfDay - (sunsetHour - transitionDuration)) / transitionDuration;
            SunIntensity = Mathf.Lerp(1f, nightSunIntensity, t);
        }
        else if (IsDay)
        {
            // Tam öğlen
            SunIntensity = 3f;
        }
        else
        {
            // Gece
            SunIntensity = nightSunIntensity;
        }
    }

    private void CheckTimeChangedEvent()
    {
        int currentMinute = GetMinute();
        if (currentMinute != lastBroadcastedMinute)
        {
            lastBroadcastedMinute = currentMinute;
            OnTimeChanged?.Invoke(GetHour(), currentMinute);
        }
    }

    private void HandleDebugInput()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            // 1 oyun saati (1f) ileri/geri alıyoruz
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                AddTime(1f);
            
            if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                AddTime(-1f);
        }
#endif
    }

    // --- DIŞ SİSTEMLER İÇİN YARDIMCI METOTLAR ---

    public int GetHour() => Mathf.FloorToInt(CurrentTimeOfDay);
    public int GetMinute() => Mathf.FloorToInt((CurrentTimeOfDay % 1f) * 60f);

    /// <summary>
    /// Zamana saat ekler veya çıkarır. (Örn: 1 saat eklemek için 1f, 30 dk için 0.5f)
    /// </summary>
    public void AddTime(float hoursToAdd)
    {
        CurrentTimeOfDay += hoursToAdd;
        
        // Eğer geri alırken 0'ın altına düşerse veya ileri alırken 24'ü geçerse düzelt
        if (CurrentTimeOfDay < 0f) CurrentTimeOfDay += 24f;
        CurrentTimeOfDay %= 24f;
    }

    /// <summary>
    /// Zamanı doğrudan spesifik bir saate ayarlar.
    /// </summary>
    public void SetTime(float newHour)
    {
        CurrentTimeOfDay = Mathf.Clamp(newHour, 0f, 24f);
    }
}