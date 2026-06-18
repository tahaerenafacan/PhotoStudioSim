using TMPro;
using UniStorm;
using UnityEngine;
using UnityEngine.Localization;

public class HUDController : MonoBehaviour
{
    [SerializeField] private HintManager hintManager;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Localization")]
    [SerializeField] private LocalizedString localizedPickupHint;
    [SerializeField] private LocalizedString localizedDropHint;

    private void Start()
    {
        CurrencyManager.Instance.OnBalanceChanged += OnBalanceChanged;
        OnBalanceChanged(CurrencyManager.Instance.GetMoney()); //Initialize

        UniStormManager.Instance.OnTimeChange += OnTimeChanged;
        OnTimeChanged(UniStormManager.Instance.GetHour(), UniStormManager.Instance.GetMinutes());

        PlayerItemHolder.Instance.OnHeldItemChanged += OnHeldItemChanged;

        PlayerInteraction.Instance.OnDetectionChanged += OnInteractionDetectionChanged;
    }

    private void OnInteractionDetectionChanged(BasePickableItem pickableItem, IInteractable interactable)
    {
        if (pickableItem != null && interactable != null)
            hintManager.OnItemChanged(interactable.InteractName, true, true, interactable.InteractHint);
        else if (pickableItem != null)
            hintManager.OnItemChanged(pickableItem.GetItemName(), false, true, null);
        else if (interactable != null)
            hintManager.OnItemChanged(interactable.InteractName, true, false, interactable.InteractHint);
        else
            hintManager.OnItemChanged(null, false, false, null);
    }

    private void OnHeldItemChanged(bool isHoldingItem, BasePickableItem holdingItem)
    {
        //TODO: isholding item show drop hint & show item name
    }

    private void OnTimeChanged(int hour, int min)
    {
        timeText.text = $"{hour:00}:{min:00}";
    }

    private void OnBalanceChanged(int newMoney)
    {
        moneyText.text = $"${newMoney:F2}";
    }

    private void SetText(TextMeshProUGUI tmp, bool active, string text = null)
    {
        if (!tmp) return;
        tmp.gameObject.SetActive(active);
        if (active && !string.IsNullOrEmpty(text))
            tmp.text = text;
    }
}
