using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class HUDController : MonoBehaviour
{
    [SerializeField] private MMDebugMenu debugMenu;
    
    [SerializeField] private TextMeshProUGUI interactionHintText;
    [SerializeField] private TextMeshProUGUI useHintText;
    [SerializeField] private TextMeshProUGUI dropHintText;
    [SerializeField] private TextMeshProUGUI heldItemNameText;
    
    [SerializeField] private LocalizedString localizedPickupHint;
    [SerializeField] private LocalizedString localizedDropHint;
    
    private string cachedPickupHint = "";
    private string cachedDropHint = "";
    
    private PlayerInteraction interaction;
    private PlayerItemHolder holder;
    
    private void OnEnable()
    {
        localizedPickupHint.StringChanged += OnPickupHintChanged;
        localizedDropHint.StringChanged += OnDropHintChanged;
    }

    private void OnDisable()
    {
        localizedPickupHint.StringChanged -= OnPickupHintChanged;
        localizedDropHint.StringChanged -= OnDropHintChanged;
    }
    
    private void OnDropHintChanged(string value)
    {
        cachedDropHint = value;
    }
    
    private void OnPickupHintChanged(string value)
    {
        cachedPickupHint = value;
    }
    
    private void Start()
    {
        interaction = PlayerInteraction.Instance;
        holder = PlayerItemHolder.Instance;
        
        SetText(interactionHintText, false);
        SetText(useHintText, false);
        SetText(dropHintText, false);
        SetText(heldItemNameText, false);
    }
    
    private void Update()
    {
        if (Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            debugMenu.ToggleMenu();
            InputManager.ToggleCursorLock();
        }
        
        if (interaction.DetectedPickable != null)
        {
            SetText(interactionHintText, true, cachedPickupHint);
        }
        else if (interaction.DetectedInteractable != null)
        {
            SetText(interactionHintText, true, $"<b>E: </b> {interaction.DetectedInteractable.InteractHint}");
        }
        else
        {
            SetText(interactionHintText, false);
        }

        if (holder.IsHoldingItem)
        {
            //SetText(heldItemNameText, true, holder.GetHeldItemName());

            string useHint = holder.GetUseHint();
            if (!string.IsNullOrEmpty(useHint))
                SetText(useHintText, true, useHint);
            else
                SetText(useHintText, false);

            SetText(dropHintText, true, cachedDropHint);
        }
        else
        {
            SetText(heldItemNameText, false);
            SetText(useHintText, false);
            SetText(dropHintText, false);
        }
    }
    
    private void SetText(TextMeshProUGUI tmp, bool active, string text="")
    {
        if (!tmp) return;
        if (active && !string.IsNullOrEmpty(text)) tmp.text = text;
        tmp.gameObject.SetActive(active);
    }
}
