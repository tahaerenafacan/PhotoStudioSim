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

    private string cachedPickupHint = "";
    private string cachedDropHint = "";

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
        CurrencyManager.Instance.OnBalanceChanged += OnBalanceChanged;
        OnBalanceChanged(CurrencyManager.Instance.GetMoney()); //Initialize

        UniStormManager.Instance.OnMinuteChanged += OnTimeChanged;
        OnTimeChanged(); //Initialize

        PlayerItemHolder.Instance.OnHeldItemChanged += OnHeldItemChanged;

        PlayerInteraction.Instance.OnDetectionChanged += OnInteractionDetectionChanged;
    }

    private void OnInteractionDetectionChanged(BasePickableItem pickableItem, IInteractable interactable)
    {
        if (pickableItem)
        {
            hintManager.OnItemChanged(pickableItem.GetItemName(), interactable!=null, true);
        }
        else hintManager.OnItemChanged(null, interactable!=null, false);
    }

    private void OnHeldItemChanged(bool isHoldingItem, BasePickableItem holdingItem)
    {
        /*if (isHoldingItem)
        {
            SetText(heldItemNameText, true, holdingItem.GetItemName());
            SetText(dropHintText, true, cachedDropHint);

            if (holdingItem.IsUseable)
            {
                string useHint = PlayerItemHolder.Instance.GetUseHint().GetLocalizedString();
                SetText(useHintText, true, useHint);
            }                
            else
            {
                SetText(useHintText, false);
            }
        }
        else
        {
            SetText(heldItemNameText, false);
            SetText(useHintText, false);
            SetText(dropHintText, false);
        }*/
    }

    private void OnTimeChanged()
    {
        timeText.text = UniStormManager.Instance.GetHour().ToString("00") + ":" + UniStormManager.Instance.GetMinutes().ToString("00");
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
