using System;
using ItemSystem;
using Newtonsoft.Json.Linq;
using SyntaxSultan.SavingSystem;
using UnityEngine;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Envanterin merkezi yöneticisi. Slotları oluşturur ve tek erişim noktası sağlar.
    ///
    /// SAHNE KURULUMU:
    ///   1. Bu scripti sahnedeki bir Manager objesine ekle.
    ///   2. StorageRoot boş bırakılırsa otomatik oluşturulur.
    ///   3. UpgradeSlotCount() ile slot sayısını runtime'da artır (max 8).
    /// </summary>
    public class InventorySystem : MonoBehaviour, IJsonSaveable
    {
        public static InventorySystem Instance { get; private set; }

        [SerializeField, Range(2, 8)]
        private int initialSlotCount = 2;

        [SerializeField, Tooltip("Envanterde saklanan itemların gizleneceği Transform. Boş = otomatik oluşturulur.")]
        private Transform storageRoot;

        private InventorySlot[] slots;

        public int SlotCount => slots.Length;

        /// <summary>Bir slot içeriği değiştiğinde → (slotIndex, yeni item veya null)</summary>
        public event Action<int, BasePickableItem> OnSlotChanged;

        /// <summary>UpgradeSlotCount başarılı olduğunda → yeni slot sayısı</summary>
        public event Action<int> OnSlotCountChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            EnsureStorageRoot();
            InitializeSlots(initialSlotCount);
        }

        private void EnsureStorageRoot()
        {
            if (storageRoot != null) return;

            // Gizli bir parent oluştur; tüm child'ları pasif tutar
            var go = new GameObject("[InventoryStorage]");
            go.SetActive(false);
            go.transform.SetParent(transform);
            storageRoot = go.transform;
        }

        private void InitializeSlots(int count)
        {
            slots = new InventorySlot[count];
            for (int i = 0; i < count; i++)
                RegisterSlot(i);
        }

        private void RegisterSlot(int index)
        {
            slots[index] = new InventorySlot();
            int captured = index; // Closure için yakalanmalı
            slots[index].OnChanged += item => OnSlotChanged?.Invoke(captured, item);
        }

        // ─────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────

        /// <summary>Belirtilen slota item saklar. Başarısız olursa false döner.</summary>
        public bool TryStore(BasePickableItem item, int slotIndex)
        {
            if (!IsValidIndex(slotIndex)) return false;
            return slots[slotIndex].TryStore(item, storageRoot);
        }

        /// <summary>Belirtilen slottan item çıkarır. Slot boşsa null döner.</summary>
        public BasePickableItem Take(int slotIndex)
        {
            if (!IsValidIndex(slotIndex)) return null;
            return slots[slotIndex].Take();
        }

        public bool IsSlotEmpty(int slotIndex) =>
            IsValidIndex(slotIndex) && slots[slotIndex].IsEmpty;

        public BasePickableItem GetItem(int slotIndex) =>
            IsValidIndex(slotIndex) ? slots[slotIndex].StoredItem : null;

        /// <summary>
        /// Slot sayısını artırır (max 8). Azaltma veri kaybı yaratır, desteklenmez.
        /// Başarı durumunda OnSlotCountChanged tetiklenir.
        /// </summary>
        public bool UpgradeSlotCount(int newCount)
        {
            int oldCount = slots.Length;
            if (newCount <= oldCount || newCount > 8)
            {
                Debug.LogWarning($"[InventorySystem] Geçersiz upgrade: mevcut={oldCount}, istek={newCount}, max=8");
                return false;
            }

            var expanded = new InventorySlot[newCount];
            Array.Copy(slots, expanded, oldCount); // Mevcut slotları ve event'lerini koru

            slots = expanded;
            for (int i = oldCount; i < newCount; i++)
                RegisterSlot(i);

            OnSlotCountChanged?.Invoke(newCount);
            return true;
        }

        private bool IsValidIndex(int index) => index >= 0 && index < slots.Length;
        
        public JToken CaptureAsJToken()
        {
            // Her slotu itemId string'i olarak saklıyoruz; boş slot = null.
            // Transform/fizik state'i saklanmıyor çünkü RetrieveFromInventory zaten sıfırlıyor.
            JArray slotArray = new JArray();
            foreach (InventorySlot slot in slots)
            {
                slotArray.Add(slot.IsEmpty ? null : slot.StoredItem.GetItemId());
            }

            JObject state = new JObject();
            state["slotCount"] = slots.Length;
            state["slots"] = slotArray;
            return state;
        }

        public void RestoreFromJToken(JToken token)
        {
            JObject state = token.ToObject<JObject>();
            int savedSlotCount = state["slotCount"].ToObject<int>();
            JArray slotArray = (JArray)state["slots"];

            ClearAllSlots();

            // Save dosyası daha fazla slot içeriyorsa (upgrade sonrası kaydedilmişse) mevcut envanteri büyüt
            if (savedSlotCount > slots.Length)
                UpgradeSlotCount(savedSlotCount);

            for (int i = 0; i < slotArray.Count; i++)
            {
                JToken idToken = slotArray[i];
                if (idToken.Type == JTokenType.Null) continue;

                RestoreItemIntoSlot(idToken.ToObject<string>(), i);
            }
        }
        
        /// <summary>
        /// Sahne yeniden yüklenmeden restore çağrılırsa (ör. quickload), önceki
        /// envanterdeki item objelerinin sahnede yetim kalmaması için temizler.
        /// </summary>
        private void ClearAllSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                BasePickableItem leftover = slots[i].Take();
                if (leftover != null) Destroy(leftover.gameObject);
            }
        }

        private void RestoreItemIntoSlot(string itemId, int slotIndex)
        {
            ItemDefinition def = ItemDatabase.GetById(itemId);
            if (def == null || def.itemPrefab == null)
            {
                Debug.LogWarning($"[InventorySystem] itemId '{itemId}' ItemDatabase'de bulunamadı, slot {slotIndex} atlandı.");
                return;
            }

            BasePickableItem instance = Instantiate(def.itemPrefab);
            instance.Initialize(def);

            // TryStore false dönerse item sahnede yetim (aktif, storage dışı) kalır — bunu tespit edip temizliyoruz
            if (!TryStore(instance, slotIndex))
            {
                Debug.LogError($"[InventorySystem] '{itemId}' slot {slotIndex}'e restore edilemedi. " +
                               $"IStorable implement ediyor mu / CanStore true mu kontrol et: {instance.GetType()}");
                Destroy(instance.gameObject);
            }
        }
    }
}