using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ShopSystem
{
    [RequireComponent(typeof(Collider))]
    public class CargoBox : BasePickableItem, IInteractable, IComplexUsable
    {
        public event Action OnCollected;

        [SerializeField] private LocalizedString openHint;
        [SerializeField] private LocalizedString boxDisplayName;

        public LocalizedString InteractHint => openHint;
        public LocalizedString InteractName => boxDisplayName;
        public bool CanInteract => containedItem != null;

        private ShopItemDefinition containedItem;

        public List<ItemInteraction> GetInteractions()
        {
            return null;
        }

        /// <summary>OrderDeliveryManager tarafından spawn anında çağrılır.</summary>
        public void Initialize(ShopItemDefinition item)
        {
            containedItem = item;
        }

        public void Interact()
        {
            if (containedItem == null || containedItem.ItemPrefab == null) return;

            SpawnContainedItem();

            OnCollected?.Invoke();
            Destroy(gameObject);
        }

        private void SpawnContainedItem()
        {
            Vector3 spawnPos = transform.position + transform.forward * 0.3f;
            Instantiate(containedItem.ItemPrefab, spawnPos, transform.rotation);
        }
    }
}