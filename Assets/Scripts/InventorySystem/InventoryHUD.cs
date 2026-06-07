using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Envanter slotlarını ekranın alt merkezinde HUD olarak gösterir.
    /// Prefab veya ek sahne kurulumu gerekmez; Canvas'ı ve slotları kendisi oluşturur.
    ///
    /// SAHNE KURULUMU:
    ///   Bu scripti sahnedeki herhangi bir objeye (örn. UIManager) ekle.
    ///   InventorySystem ve InventoryController aynı sahnede olmalıdır.
    ///   İsteğe bağlı: targetCanvas doldurmak yerine kendi Canvas'ını oluşturur.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private InventorySlotUI slotUIPrefab;

        [Header("Renkler")]
        [SerializeField] private Color normalBgColor   = new Color(0.05f, 0.05f, 0.05f, 0.80f);
        [SerializeField] private Color selectedBgColor = new Color(0.95f, 0.78f, 0.10f, 0.90f);
        [SerializeField] private Color iconTintColor   = Color.white;
        
        private readonly List<InventorySlotUI> slotUIs = new();
        
        private InventorySystem inventory;
        private InventoryController controller;

        private void Start()
        {
            inventory  = InventorySystem.Instance;
            controller = InventoryController.Instance;

            BuildSlotContainer(inventory.SlotCount);

            inventory.OnSlotChanged      += HandleSlotChanged;
            inventory.OnSlotCountChanged += HandleSlotCountChanged;
            controller.OnActiveSlotChanged += HandleActiveSlotChanged;

            HandleActiveSlotChanged(controller.ActiveSlotIndex);
        }

        private void OnDestroy()
        {
            if (inventory  != null)
            {
                inventory.OnSlotChanged      -= HandleSlotChanged;
                inventory.OnSlotCountChanged -= HandleSlotCountChanged;
            }
            if (controller != null)
                controller.OnActiveSlotChanged -= HandleActiveSlotChanged;
        }

        private void BuildSlotContainer(int count)
        {
            FunctionLibrary.DestroyChildren(slotContainer);

            slotUIs.Clear();
            
            for (int i = 0; i < count; i++)
                AddSlotUI(i);
        }

        private void AddSlotUI(int index)
        {
            InventorySlotUI slotUI = Instantiate(slotUIPrefab, slotContainer.transform);

            slotUI.Initialize(index + 1, normalBgColor, selectedBgColor, iconTintColor);
            slotUIs.Add(slotUI);
        }

        // ─────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────

        private void HandleSlotChanged(int slotIndex, BasePickableItem item)
        {
            if (slotIndex < 0 || slotIndex >= slotUIs.Count) return;

            Sprite icon = (item is IStorable storable) ? storable.Icon : null;
            slotUIs[slotIndex].SetIcon(icon);
        }

        private void HandleSlotCountChanged(int newCount)
        {
            BuildSlotContainer(newCount);

            // Mevcut slot içeriklerini yeniden senkronize et
            for (int i = 0; i < newCount; i++)
            {
                var item = inventory.GetItem(i);
                Sprite icon = (item is IStorable storable) ? storable.Icon : null;
                slotUIs[i].SetIcon(icon);
            }

            HandleActiveSlotChanged(controller.ActiveSlotIndex);
        }

        private void HandleActiveSlotChanged(int index)
        {
            for (int i = 0; i < slotUIs.Count; i++)
                slotUIs[i].SetSelected(i == index);
        }
    }
}