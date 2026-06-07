using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlaylistItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trackNameText;
    [SerializeField] private Image trackActiveImage;
    
    private Button button;
 
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetTrackName(string trackName)
    {
        trackNameText.text = trackName;
    }
    
    public void SetTrackActive(bool active)
    {
        trackActiveImage.gameObject.SetActive(active);
    }
    
    public void SetOnClick(System.Action callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback?.Invoke());
    }
}
