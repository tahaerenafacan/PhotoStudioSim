using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Tek bir slot için arka plan, ikon ve numara etiketini yönetir.
    /// InventoryHUD tarafından runtime'da oluşturulur; doğrudan kullanım gerekmez.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image           background;
        [SerializeField] private Image           iconImage;
        [SerializeField] private TextMeshProUGUI slotLabel;

        private Color normalColor;
        private Color activeColor;

        public void Initialize(int slotNumber, Color normalColor, Color activeColor, Color iconTint)
        {
            this.normalColor = normalColor;
            this.activeColor = activeColor;
            slotLabel.text = slotNumber.ToString();
        }

        public void SetIcon(Sprite sprite)
        {
            iconImage.sprite  = sprite;
            iconImage.enabled = sprite != null;
        }

        public void SetSelected(bool selected) => background.color = selected ? activeColor : normalColor;
    }
}