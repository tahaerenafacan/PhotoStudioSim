using SyntaxSultan.CameraSystem;
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
    
    [Header("Physical Camera Display")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI isoText;
    [SerializeField] private TextMeshProUGUI apertureText;
    [SerializeField] private TextMeshProUGUI shutterText;
    [SerializeField] private TextMeshProUGUI exposureMeterText;
    [SerializeField] private TextMeshProUGUI activeParamText;

    public void UpdateZoomDisplay(float zoomRatio)
    {
        if (zoomRatioDisplay == null) return;
        zoomRatioDisplay.text = $"{zoomRatio:F1}x";
    }

    public void SetScreenActive(bool isActive)
    {
        if (screenCanvas != null)
        {
            screenCanvas.enabled = isActive;
        }
    }

    public void SetFlashImage(bool isFlashOn)
    {
        flashImage.sprite = isFlashOn ? flashOnSprite : flashOffSprite;
    }
    
    /// <summary>
    /// Fiziksel kamera değerlerini viewfinder HUD'ına yansıtır.
    /// activeParam: null ise AUTO modda.
    /// </summary>
    public void UpdatePhysicalDisplay(CameraPhysicalSettings s, CameraMode mode, float ev100, ManualParameter? activeParam)
    {
        if (modeText != null)
            modeText.text = mode == CameraMode.Auto ? "AUTO" : "M";

        if (isoText != null)
            isoText.text = $"ISO {(int)s.ISO}";

        if (apertureText != null)
            apertureText.text = $"f/{s.Aperture:F1}";

        if (shutterText != null)
        {
            int denom = Mathf.RoundToInt(1f / s.ShutterSpeed);
            shutterText.text = $"1/{denom}";
        }

        if (exposureMeterText != null)
        {
            // 10 EV indoor referans; pozitif → aşık, negatif → karanlık
            float deviation = ev100 - 10f;
            string sign = deviation >= 0f ? "+" : "";
            exposureMeterText.text  = $"EV {sign}{deviation:F1}";
            exposureMeterText.color = Mathf.Abs(deviation) < 1f ? Color.white : Color.red;
        }

        if (activeParamText != null)
            activeParamText.text = activeParam.HasValue ? $"[{activeParam.Value}]" : "";
    }
}
