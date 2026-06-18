using UnityEngine;

public class DaySummaryUI : MonoBehaviour
{
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Evo.UI.Button nextDayButton;
    [SerializeField] private Bed bedToSleep;

    private void Awake()
    {
        nextDayButton.onClick.AddListener(OnNextDayButtonPressed);
    }

    private void Start()
    {
        bedToSleep.OnSleep += ShowDaySummary;
    }

    private void ShowDaySummary()
    {
        uiContainer.gameObject.SetActive(true);
        InputManager.SetCursorLock(false);
    }

    private void HideDaySummary()
    {
        uiContainer.gameObject.SetActive(false);
        InputManager.SetCursorLock(true);
    }

    private void OnNextDayButtonPressed()
    {
        HideDaySummary();
        GameManager.Instance.EndDayAndSleep();
    }
}
