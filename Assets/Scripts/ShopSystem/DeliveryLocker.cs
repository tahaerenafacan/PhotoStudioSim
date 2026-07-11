using System.Collections.Generic;
using UnityEngine;

namespace SyntaxSultan.ShopSystem
{
    public class DeliveryLocker : MonoBehaviour, IDeliveryLocker
    {
        [SerializeField] private Transform[] slotPoints;

        // Occupied slot -> içindeki kutu referansı (locker'ın kendi SSOT'u)
        private readonly Dictionary<Transform, CargoBox> occupiedSlots = new();

        public bool HasFreeSlot => GetFreeSlot() != null;

        private void OnEnable() => OrderDeliveryManager.Instance?.RegisterLocker(this);
        private void OnDisable() => OrderDeliveryManager.Instance?.UnregisterLocker(this);

        public bool TryReserveSlot(out Transform slotTransform)
        {
            slotTransform = GetFreeSlot();
            return slotTransform != null;
        }

        public void ReleaseSlot(Transform slotTransform)
        {
            occupiedSlots.Remove(slotTransform);
        }

        /// <summary>Spawn edilen kutuyu slot işgaline kaydeder. CargoBox spawn edildikten sonra çağrılır.</summary>
        public void MarkSlotOccupied(Transform slotTransform, CargoBox box)
        {
            occupiedSlots[slotTransform] = box;
        }

        private Transform GetFreeSlot()
        {
            foreach (var slot in slotPoints)
            {
                if (slot != null && !occupiedSlots.ContainsKey(slot))
                    return slot;
            }
            return null;
        }
    }
}