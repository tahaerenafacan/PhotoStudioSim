using System;
using System.Collections.Generic;
using UnityEngine;

namespace SyntaxSultan.ShopSystem
{
    /// <summary>
    /// Tüm bekleyen siparişlerin ve kayıtlı dolapların tek doğruluk kaynağı (SSOT).
    ///
    /// AKIŞ:
    ///   ShopApp.Buy() -> CreateOrder() -> Update() içinde tick -> süresi dolunca
    ///   uygun IDeliveryLocker bulunur -> CargoBox spawn edilir.
    ///
    /// SAHNE KURULUMU: Herhangi bir persistent manager objesine ekle (GameManager yanı önerilir).
    /// </summary>
    public class OrderDeliveryManager : MonoBehaviour
    {
        public static OrderDeliveryManager Instance { get; private set; }

        [SerializeField] private CargoBox cargoBoxPrefab;

        private readonly List<PendingOrder> pendingOrders = new();
        private readonly List<IDeliveryLocker> registeredLockers = new();
        // Slot yokken OnDeliveryFailedNoSlot'un her frame tekrar tetiklenip UI'ı spamlamasını önler.
        private readonly HashSet<string> ordersNotifiedNoSlot = new();

        public event Action<PendingOrder> OnOrderCreated;
        public event Action<PendingOrder> OnOrderDelivered;
        /// <summary>Sipariş hazır ama hiçbir dolapta boş slot yoksa tetiklenir (UI uyarısı için).</summary>
        public event Action<PendingOrder> OnDeliveryFailedNoSlot;

        public IReadOnlyList<PendingOrder> PendingOrders => pendingOrders;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            // Geriye doğru iterasyon: TryDeliver içinde listeden eleman silinebilir.
            for (int i = pendingOrders.Count - 1; i >= 0; i--)
            {
                var order = pendingOrders[i];
                order.Tick(Time.deltaTime);

                if (order.IsReady)
                {
                    TryDeliver(order);
                }
            }
        }

        // ── Public API ───────────────────────────────────────────────

        public PendingOrder CreateOrder(ShopItemDefinition item)
        {
            var order = new PendingOrder(item);
            pendingOrders.Add(order);
            OnOrderCreated?.Invoke(order);
            return order;
        }

        public void RegisterLocker(IDeliveryLocker locker)
        {
            if (!registeredLockers.Contains(locker))
                registeredLockers.Add(locker);
        }

        public void UnregisterLocker(IDeliveryLocker locker)
        {
            registeredLockers.Remove(locker);
        }

        // ── Private ──────────────────────────────────────────────────

        /// <summary>
        /// Boş slotu olan ilk dolabı bulup kutuyu spawn eder.
        /// Slot bulunamazsa sipariş listede kalır; her Update'te tekrar denenir
        /// (oyuncu dolabı boşaltana kadar teslimat bekler — kutu kaybolmaz).
        /// </summary>
        private void TryDeliver(PendingOrder order)
        {
            foreach (var locker in registeredLockers)
            {
                if (!locker.TryReserveSlot(out Transform slot)) continue;

                var box = Instantiate(cargoBoxPrefab, slot.position, slot.rotation);
                box.Initialize(order.Item);

                if (locker is DeliveryLocker concreteLocker)
                    concreteLocker.MarkSlotOccupied(slot, box);

                box.OnCollected += () => locker.ReleaseSlot(slot);

                pendingOrders.Remove(order);
                ordersNotifiedNoSlot.Remove(order.OrderId);
                OnOrderDelivered?.Invoke(order);
                return;
            }

            // Hiçbir dolapta yer yoksa: sadece ilk denemede bildir, sipariş kuyrukta kalmaya devam etsin.
            if (ordersNotifiedNoSlot.Add(order.OrderId))
                OnDeliveryFailedNoSlot?.Invoke(order);
        }
    }
}