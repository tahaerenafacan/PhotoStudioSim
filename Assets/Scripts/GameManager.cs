using System;
using UniStorm;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct TimeStruct
    {
        public int  hour;
        public int minute;
    }
    
    public static GameManager Instance { get; private set; }

    public event Action OnGamePause;
    public event Action OnGameResume;
    public event Action OnDayEndedEvent;
    
    [SerializeField] private TimeStruct dayEndTime;
    [SerializeField] private TimeStruct dayStartTime;
    
    private bool isGamePaused;
    private bool isDayEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 144;
        UniStormManager.Instance.OnTimeChange += UniStormManager_OnMinuteChanged;
    }

    private void UniStormManager_OnMinuteChanged(int hour, int minute)
    {
        if (hour >= dayEndTime.hour && minute >= dayEndTime.minute)
        {
            isDayEnded = true;
            OnDayEndedEvent?.Invoke();
            UniStormManager.Instance.SetTimeFlow(false);
        }
        else if (isDayEnded)
        {
            UniStormManager.Instance.SetTimeFlow(true);
            isDayEnded = false;
        }
    }

    private void Update()
    {
        Cheats();
    }

    private void Cheats()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            InputManager.ToggleCursorLock();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode loadSceneMode)
    {
        if (s.name.Equals("PrototypeScene")) InputManager.SetCursorLock(true);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        HandleGamePause(isGamePaused);
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        HandleGamePause(isGamePaused);
    }

    public void PauseGame()
    {
        isGamePaused = true;
        HandleGamePause(isGamePaused);
    }

    private void HandleGamePause(bool isPaused)
    {
        if (isPaused)
        {
            InputManager.SetCursorLock(false);
            OnGamePause?.Invoke();
            Time.timeScale = 0f;
        }
        else
        {
            InputManager.SetCursorLock(true);
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }
    }

    public void EndDayAndSleep()
    {
        UniStormManager.Instance.NextDay();
        UniStormManager.Instance.SetTime(dayStartTime.hour, dayStartTime.minute);
    }
}
