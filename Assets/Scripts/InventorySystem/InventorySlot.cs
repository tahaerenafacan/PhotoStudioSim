using System;
using UnityEngine;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Tek bir slot için veri modeli. MonoBehaviour değildir.
    /// Item'ı yok etmez; sahneye gizler ve geri getirir.
    /// InventorySystem tarafından oluşturulur ve yönetilir.
    /// </summary>
    public class InventorySlot
    {
        public BasePickableItem StoredItem { get; private set; }
        public bool IsEmpty => StoredItem == null;

        /// <summary>İçerik değiştiğinde tetiklenir. null → slot boşaldı.</summary>
        public event Action<BasePickableItem> OnChanged;

        /// <summary>
        /// Item'ı slota saklar. Slot doluysa veya item IStorable değilse false döner.
        /// Başarı durumunda item sahneye gizlenir (yok edilmez).
        /// </summary>
        public bool TryStore(BasePickableItem item, Transform storageRoot)
        {
            if (!IsEmpty) return false;
            if (item is not IStorable storable || !storable.CanStore) return false;

            StoredItem = item;
            item.StoreInInventory(storageRoot);
            OnChanged?.Invoke(StoredItem);
            return true;
        }

        /// <summary>
        /// Item'ı slottan çıkarır ve sahneye geri getirir.
        /// Ardından çağıran taraf PlayerItemHolder.TryPickup() yapmalıdır.
        /// </summary>
        public BasePickableItem Take()
        {
            if (IsEmpty) return null;

            var item = StoredItem;
            StoredItem = null;
            item.RetrieveFromInventory();
            OnChanged?.Invoke(null);
            return item;
        }
    }
}