using UnityEngine;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Envanter slotları ile oyuncu eli arasındaki etkileşim mantığını yönetir.
    ///
    /// KARAR TABLOSU (slot tuşuna basıldığında):
    ///   El dolu + Slot boş  + IStorable → slota koy
    ///   El dolu + Slot dolu             → takas
    ///   El boş  + Slot dolu             → ele al
    ///   El boş  + Slot boş              → işlem yok
    ///
    ///   Scroll → Yalnızca aktif slot görselini değiştirir, etkileşim yapmaz.
    ///
    /// BAĞIMLILIKLAR: InventorySystem, PlayerItemHolder, InventoryInputHandler (Inspector)
    /// </summary>
    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance { get; private set; }

        [SerializeField] private InventoryInputHandler inputHandler;

        private InventorySystem inventory;
        private int activeSlotIndex;

        public int ActiveSlotIndex => activeSlotIndex;

        /// <summary>Aktif slot değiştiğinde yeni index ile tetiklenir.</summary>
        public event System.Action<int> OnActiveSlotChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            inventory = InventorySystem.Instance;

            inputHandler.Initialize(inventory.SlotCount);
            inputHandler.OnSlotKeyPressed += HandleSlotInteraction;

            // Slot sayısı upgrade edildiğinde input handler'ı güncelle
            inventory.OnSlotCountChanged += inputHandler.SetMaxSlots;
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnSlotKeyPressed -= HandleSlotInteraction;
            }
            if (inventory != null)
                inventory.OnSlotCountChanged -= inputHandler.SetMaxSlots;
        }

        // ─────────────────────────────────────────────────────────────
        // Etkileşim Mantığı
        // ─────────────────────────────────────────────────────────────

        private void HandleSlotInteraction(int slotIndex)
        {
            if (slotIndex >= inventory.SlotCount) return;

            SetActiveSlot(slotIndex);

            bool isHolding   = PlayerItemHolder.Instance.IsHoldingItem;
            bool isSlotEmpty = inventory.IsSlotEmpty(slotIndex);

            if      ( isHolding &&  isSlotEmpty) TryStoreHeldItem(slotIndex);
            else if ( isHolding && !isSlotEmpty) SwapHeldWithSlot(slotIndex);
            else if (!isHolding && !isSlotEmpty) TakeFromSlot(slotIndex);
        }

        private void TryStoreHeldItem(int slotIndex)
        {
            var item = PlayerItemHolder.Instance.DetachForStorage();
            if (item == null) return;

            if (!inventory.TryStore(item, slotIndex))
            {
                // IStorable değil veya CanStore=false → eli geri ver
                PlayerItemHolder.Instance.TryPickup(item);
                Debug.Log($"[InventoryController] '{item.name}' envantere koyulamaz. IStorable ve CanStore'u kontrol et.");
            }
        }

        private void TakeFromSlot(int slotIndex)
        {
            var item = inventory.Take(slotIndex);
            if (item != null)
                PlayerItemHolder.Instance.TryPickup(item);
        }

        /// <summary>
        /// Eldeki ile slottaki itemi takas eder.
        /// Eldeki item IStorable değilse rollback yapılır, her iki item özgün yerine döner.
        /// </summary>
        private void SwapHeldWithSlot(int slotIndex)
        {
            var fromHand = PlayerItemHolder.Instance.DetachForStorage();
            if (fromHand == null) return;

            var fromSlot = inventory.Take(slotIndex); // Slottan çıkar, aktif hale getir

            if (!inventory.TryStore(fromHand, slotIndex))
            {
                // Eldeki item slota koyulamıyor: her şeyi geri al
                if (fromSlot != null)
                    inventory.TryStore(fromSlot, slotIndex);

                PlayerItemHolder.Instance.TryPickup(fromHand);
                Debug.Log($"[InventoryController] '{fromHand.name}' takas için envantere koyulamaz.");
                return;
            }

            if (fromSlot != null)
                PlayerItemHolder.Instance.TryPickup(fromSlot);
        }

        private void SetActiveSlot(int index)
        {
            if (activeSlotIndex == index) return;
            activeSlotIndex = index;
            OnActiveSlotChanged?.Invoke(activeSlotIndex);
        }
    }
}