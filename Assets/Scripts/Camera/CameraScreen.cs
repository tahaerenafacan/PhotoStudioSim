using TMPro;
using UnityEngine;

public class CameraScreen : MonoBehaviour
{
    [SerializeField] private Canvas screenCanvas;
    
    [Header("Zoom")]
    [SerializeField] private TextMeshProUGUI zoomRatioDisplay;

    [Header("Flash")]
    [SerializeField] private UnityEngine.UI.Image flashImage;
    [SerializeField] private Sprite flashOnSprite;
    [SerializeField] private Sprite flashOffSprite;

    public void UpdateZoomDisplay(float currentFOV, float normalFov)
    {
        if (zoomRatioDisplay == null) return;

        float zoomRatio = normalFov / currentFOV;
        zoomRatioDisplay.text = $"{zoomRatio:F1}x";
    }

    public void SetScreenActive(bool isActive)
    {
        if (screenCanvas != null)
        {
            screenCanvas.gameObject.SetActive(isActive);
        }
    }

    public void SetFlashImage(bool isFlashOn)
    {
        flashImage.sprite = isFlashOn ? flashOnSprite : flashOffSprite;
    }
}
