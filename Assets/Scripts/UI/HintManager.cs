using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    [Header("Item Name Settings")]
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    [SerializeField] private CanvasGroup itemNameGroup;
    [SerializeField] private LayoutElement itemNameLayout;

    [Header("Pickup Hint Settings")]
    [SerializeField] private CanvasGroup pickupHint;
    [SerializeField] private LayoutElement pickupLayout;
    [SerializeField] private LocalizedString pickupLocalizedString;
    [SerializeField] private TMPro.TextMeshProUGUI pickupHintText;

    [Header("Interact Hint Settings")] 
    [SerializeField] private TMPro.TextMeshProUGUI interactHintText;
    [SerializeField] private CanvasGroup interactHint;
    [SerializeField] private LayoutElement interactLayout;
    
    private void Start()
    {
        OnItemChanged(null, false, false, null);
    }

    public void OnItemChanged(LocalizedString itemName, bool interact, bool pickup, LocalizedString interactText)
    {
        string resolvedName = itemName?.GetLocalizedString();
        if (string.IsNullOrEmpty(resolvedName))
        {
            ToggleUIElement(itemNameGroup, itemNameLayout, false);
        }
        else
        {
            itemNameText.text = resolvedName;
            ToggleUIElement(itemNameGroup, itemNameLayout, true);
        }

        string resolvedInteract = interactText?.GetLocalizedString();
        if (!string.IsNullOrEmpty(resolvedInteract))
        {
            interactHintText.text = $"<sprite name=\"f\"/> {resolvedInteract}";
        }
        
        string resolvedPickup = pickupLocalizedString?.GetLocalizedString();
        if (!string.IsNullOrEmpty(resolvedPickup))
        {
            pickupHintText.text = $"<sprite name=\"e\"/> {resolvedPickup}";
        }

        ToggleUIElement(interactHint, interactLayout, interact);
        ToggleUIElement(pickupHint, pickupLayout, pickup);
    }
    
    private void ToggleUIElement(CanvasGroup group, LayoutElement layout, bool show)
    {
        if (group != null)
        {
            group.alpha = show ? 1f : 0f;
            group.blocksRaycasts = show;
            group.interactable = show;
        }

        if (layout != null)
        {
            // Eğer show = false ise, ignoreLayout = true olacak (Layout alanı çökecek)
            layout.ignoreLayout = !show;
        }
    }
}
