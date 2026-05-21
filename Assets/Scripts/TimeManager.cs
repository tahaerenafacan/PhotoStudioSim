using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float dayDurationInRealSeconds = 1440f; 
    [Tooltip("Oyun başladığında saat kaç olacak? (0-24)")]
    [SerializeField, Range(0f, 24f)] private float initialHour = 6f;

    [Header("Day & Night Settings")]
    [SerializeField, Range(0f, 24f)] private float sunriseHour = 6f; // Gündoğumu
    [SerializeField, Range(0f, 24f)] private float sunsetHour = 18f; // Günbatımı
    
    // Public Properties
    public int CurrentDay { get; private set; } = 1;
    public float CurrentTimeOfDay { get; private set; } // 0 - 24
    public float NormalizedDayProgress => CurrentTimeOfDay / 24f; 
    public bool IsDay { get; private set; } = true;
    public float SunIntensity { get; private set; } = 1f;

    public float SunriseHour => sunriseHour;
    public float SunsetHour => sunsetHour;
    public bool IsTimeRunning { get; private set; } = true;

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

    private void Start()
    {
        UniStormManager.
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
        if (!IsTimeRunning) return;

        CurrentTimeOfDay += Time.deltaTime * timeMultiplier;

        // Gece yarısı geçişini engelle; yatağa basılana kadar 23:59'da bekle
        const float dayEndTime = 23f + (59f / 60f);
        if (CurrentTimeOfDay >= dayEndTime)
        {
            CurrentTimeOfDay = dayEndTime;
            IsTimeRunning = false;
        }
    }

    private void UpdateDayNightCycle()
    {
        IsDay = CurrentTimeOfDay >= sunriseHour && CurrentTimeOfDay < sunsetHour;
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

    /// <summary>
    /// Yatağa tıklandığında çağrılır.
    /// Günü ilerletir, zamanı initialHour'a döndürür ve akışı yeniden başlatır.
    /// OnDayChanged event'i tetikler.
    /// </summary>
    public void SleepAndAdvanceDay()
    {
        CurrentDay++;
        CurrentTimeOfDay = initialHour;
        IsTimeRunning = true;
        lastBroadcastedMinute = -1; // Bir sonraki frame'de OnTimeChanged'i zorla tetikle
        OnDayChanged?.Invoke(CurrentDay);
    }
}