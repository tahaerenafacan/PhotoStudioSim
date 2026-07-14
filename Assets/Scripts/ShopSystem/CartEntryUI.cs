using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SyntaxSultan.ShopSystem;

namespace SyntaxSultan.ComputerSystem.Apps
{
    public class CartEntryUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI quantityLabel;
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Evo.UI.Button incrementButton;
        [SerializeField] private Evo.UI.Button decrementButton;

        [Header("Fiyat Renkleri")]
        [SerializeField] private Color affordableColor = Color.green;
        [SerializeField] private Color unaffordableColor = Color.red;

        public void Setup(ShopItemDefinition item, int quantity, bool isAffordable, Action onIncrement, Action onDecrement)
        {
            if (iconImage != null) iconImage.sprite = item.Icon;
            if (nameLabel != null) nameLabel.text = item.ItemName.GetLocalizedString();
            if (quantityLabel != null) quantityLabel.text = quantity.ToString();

            if (priceLabel != null)
            {
                priceLabel.text = $"${item.price * quantity:F2}";
                priceLabel.color = isAffordable ? affordableColor : unaffordableColor;
            }

            incrementButton.onClick.RemoveAllListeners();
            incrementButton.onClick.AddListener(() => onIncrement?.Invoke());

            decrementButton.onClick.RemoveAllListeners();
            decrementButton.onClick.AddListener(() => onDecrement?.Invoke());
        }
    }
}