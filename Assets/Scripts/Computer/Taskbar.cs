using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    public class Taskbar : MonoBehaviour
    {

        [SerializeField] private ComputerConnectionUI connectionUI;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private Evo.UI.Button connectionButton;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        void Awake()
        {
            closeButton.onClick.AddListener(CloseComputer);
            connectionButton.onClick.AddListener(connectionUI.ToggleConnectionPanel);
        }

        void Start()
        {
            UniStorm.UniStormManager.Instance.OnTimeChange += UpdateTimeDisplay;
        }

        private void UpdateTimeDisplay(int hour, int minute)
        {
            timeText.text = $"{hour:00}:{minute:00}";
        }

        private void CloseComputer()
        {
            Computer.Instance.Shutdown();
        }
    }

}