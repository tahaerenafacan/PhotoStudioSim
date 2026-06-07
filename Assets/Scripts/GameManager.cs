using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnGamePause;
    public event Action OnGameResume;
    
    private bool isGamePaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 144;
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
}
