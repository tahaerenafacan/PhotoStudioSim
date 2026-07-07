using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Evo.UI.Button newGameButton;
    [SerializeField] private StudioNamePopupController studioNamePopup;

    private void Awake()
    {
        newGameButton.onClick.AddListener(OnNewGameClicked);
    }

    public void OnNewGameClicked()
    {
        // If there are existing save show it will overwrite it, so we should ask for confirmation first.
        // For now, we will just show the studio name popup directly.
        OnCreateStudioClicked();
    }
    private void OnCreateStudioClicked()
    {
        studioNamePopup.Show("Photo Studio Simulator");
        studioNamePopup.OnConfirmed += HandleStudioNameConfirmed;
    }

    private void HandleStudioNameConfirmed(string studioName)
    {
        Debug.Log($"Studio name confirmed: {studioName}");
        GameManager.Instance.StudioName = studioName;
        UnityEngine.SceneManagement.SceneManager.LoadScene("PrototypeScene");
    }
}