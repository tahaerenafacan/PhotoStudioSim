using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private PauseMenuUI pauseMenuUI;
    private SettingsUI settingsUI;

    private bool isGamePaused = false;

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
        pauseMenuUI = FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);    
        settingsUI = FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
    }

    public void TogglePauseGame()
    {
        if (pauseMenuUI == null) return;

        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            InputManager.SetCursorLock(false);
            pauseMenuUI.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            InputManager.SetCursorLock(true);
            pauseMenuUI.gameObject.SetActive(false);
            settingsUI.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void OpenSettings()
    {
        if (settingsUI == null) return;

        settingsUI.gameObject.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsUI == null) return;

        settingsUI.gameObject.SetActive(false);
    }
}
