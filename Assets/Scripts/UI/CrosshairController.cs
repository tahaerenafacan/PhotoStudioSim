using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Image")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite defaultCrosshair;
    [SerializeField] private Sprite handCrosshair;
    [SerializeField] private Sprite interactCrosshair;

    [Header("Crosshair Scale Animation")]
    [SerializeField] private float normalSize = 32f;
    [SerializeField] private float highlightSize = 42f;
    [SerializeField] private float sizeSpeed = 10f;

    private bool isCrosshairEnabled = true;

    private void Update()
    {
        UpdateCrosshairSprite();
        UpdateCrosshairSize();
    }

    private void UpdateCrosshairSprite()
    {
        if (!crosshairImage) return;

        var interaction = PlayerInteraction.Instance;

        if (!interaction.IsInteractionEnabled)
        {
            crosshairImage.gameObject.SetActive(false);
            isCrosshairEnabled = false;
        }
        else if (interaction.IsInteractionEnabled && !isCrosshairEnabled)
        {
            crosshairImage.gameObject.SetActive(true);
            isCrosshairEnabled=true;
        }


        if (interaction.DetectedPickable != null)
            crosshairImage.sprite = handCrosshair;
        else if (interaction.DetectedInteractable != null)
            crosshairImage.sprite = interactCrosshair;
        else
            crosshairImage.sprite = defaultCrosshair;
    }

    private void UpdateCrosshairSize()
    {
        if (!crosshairImage) return;

        float targetSize = PlayerInteraction.Instance.HasDetection ? highlightSize : normalSize;
        float currentSize = crosshairImage.rectTransform.sizeDelta.x;
        float newSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * sizeSpeed);

        crosshairImage.rectTransform.sizeDelta = new Vector2(newSize, newSize);
    }

    
}