using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SyntaxSultan.ShopSystem;

namespace SyntaxSultan.ComputerSystem.Apps
{
    /// <summary>
    /// Tek bir ShopItemDefinition'ı temsil eden grid kartı.
    /// +/- stepper doğrudan ShoppingCart miktarını değiştirir; kart kendi lokal state tutmaz (SSOT).
    /// </summary>
    public class ShopItemEntryUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Evo.UI.Button addButton;

        public void Setup(ShopItemDefinition item, ShoppingCart cart)
        {
            if (iconImage != null) iconImage.sprite = item.Icon;
            if (nameLabel != null) nameLabel.text = item.ItemName.GetLocalizedString();
            if (priceLabel != null) priceLabel.text = $"${item.price}";

            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() =>
            {
                cart.Increment(item);
            });
        }
    }
}