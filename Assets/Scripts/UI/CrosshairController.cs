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
    private PlayerInteraction interaction;

    private void Start()
    {
        interaction = PlayerInteraction.Instance;
        interaction.OnShouldCheckInteractionStateChanged += PlayerInteract_CheckInteractChanged;
        interaction.OnDetectionChanged += PlayerInteract_HandleDetectionChanged;
    }

    private void PlayerInteract_CheckInteractChanged(bool checkInteraction)
    {
        isCrosshairEnabled = checkInteraction;
        crosshairImage.gameObject.SetActive(checkInteraction);
    }
    
    private void PlayerInteract_HandleDetectionChanged(IPickable pickable, IInteractable interactable)
    {
        if (pickable != null)
            crosshairImage.sprite = handCrosshair;
        else if (interactable != null)
            crosshairImage.sprite = interactCrosshair;
        else
            crosshairImage.sprite = defaultCrosshair;
    }

    private void Update()
    {
        if (!isCrosshairEnabled) return;
        UpdateCrosshairSize();
    }

    private void UpdateCrosshairSize()
    {
        float targetSize = interaction.HasDetection ? highlightSize : normalSize;
        float currentSize = crosshairImage.rectTransform.sizeDelta.x;
        float newSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * sizeSpeed);

        crosshairImage.rectTransform.sizeDelta = new Vector2(newSize, newSize);
    }

    private void OnDestroy()
    {
        interaction.OnDetectionChanged -= PlayerInteract_HandleDetectionChanged;
        interaction.OnShouldCheckInteractionStateChanged -= PlayerInteract_CheckInteractChanged;
    }
}