using UnityEngine;
using System;
using UnityEngine.InputSystem;
using OccaSoftware.SuperSimpleSkybox.Runtime;

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
    [SerializeField] private float daySunIntensity = 3f; // Gündüz güneş ışığı gücü

    [Header("Sky")]
    [SerializeField] private Sun sun;
    [SerializeField] private Moon moon;
    [SerializeField, Range(-90f, 90f)] private float skyLatitudeOffset = 30f;
    
    // Public Properties
    public int CurrentDay { get; private set; } = 1;
    public float CurrentTimeOfDay { get; private set; } // 0 ile 24 arası sürekli akan zaman
    public float DayProgress => CurrentTimeOfDay / 24f; // 0 ile 1 arası 
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

    private void Update()
    {
        UpdateTime();
        UpdateDayNightCycle();
        UpdateSkyObjects();
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

        // Güneş ışığı yoğunluğunu yumuşak (Lerp) bir şekilde hesapla
        if (CurrentTimeOfDay >= sunriseHour && CurrentTimeOfDay < sunriseHour + transitionDuration)
        {
            // Gündoğumu geçişi
            float t = (CurrentTimeOfDay - sunriseHour) / transitionDuration;
            SunIntensity = Mathf.Lerp(nightSunIntensity, daySunIntensity, t);
        }
        else if (CurrentTimeOfDay >= sunsetHour - transitionDuration && CurrentTimeOfDay < sunsetHour)
        {
            // Günbatımı geçişi
            float t = (CurrentTimeOfDay - (sunsetHour - transitionDuration)) / transitionDuration;
            SunIntensity = Mathf.Lerp(daySunIntensity, nightSunIntensity, t);
        }
        else if (IsDay)
        {
            // Tam öğlen
            SunIntensity = daySunIntensity;
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

    private void UpdateSkyObjects()
    {
        UpdateSunRotation();
        UpdateMoonRotation();
    }

    private void UpdateSunRotation()
    {
        if (sun == null) return;

        // Gündoğumu (0°) → öğle (90°) → günbatımı (180°)
        float t = Mathf.InverseLerp(sunriseHour, sunsetHour, CurrentTimeOfDay);
        float xAngle = Mathf.Lerp(0f, 180f, t);
        sun.transform.rotation = Quaternion.Euler(xAngle, skyLatitudeOffset, 0f);
    }

    private void UpdateMoonRotation()
    {
        if (moon == null) return;

        // Gece yayı: günbatımı → gece yarısı → gündoğumu (24:00 sınırını aşarak hesapla)
        float nightDuration = (24f - sunsetHour) + sunriseHour;
        float nightProgress = CurrentTimeOfDay >= sunsetHour
            ? (CurrentTimeOfDay - sunsetHour) / nightDuration
            : (24f - sunsetHour + CurrentTimeOfDay) / nightDuration;

        float xAngle = Mathf.Lerp(0f, 180f, Mathf.Clamp01(nightProgress));
        moon.transform.rotation = Quaternion.Euler(xAngle, skyLatitudeOffset, 0f);
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