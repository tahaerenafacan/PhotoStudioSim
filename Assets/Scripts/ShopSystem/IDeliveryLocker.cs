using UnityEngine;

namespace SyntaxSultan.ShopSystem
{
    public interface IDeliveryLocker
    {
        bool HasFreeSlot { get; }

        /// <summary>Boş bir slot ayırır ve dünya pozisyonunu döner. Slot yoksa false döner.</summary>
        bool TryReserveSlot(out Transform slotTransform);

        /// <summary>Kutu alındığında/kaldırıldığında slotu tekrar boşa çıkarır.</summary>
        void ReleaseSlot(Transform slotTransform);
    }
}