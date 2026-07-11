using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SyntaxSultan.ShopSystem;

namespace SyntaxSultan.ComputerSystem.Apps
{
    /// <summary>
    /// Mağaza uygulaması.
    ///
    /// AKIŞ:
    ///   Kategori panelleri sahnede önceden hazır (spawn edilmez); her kategori kendi
    ///   itemContainer'ına bir kez item spawn eder. Sekme değişince sadece panelRoot
    ///   görünürlüğü değişir (item'lar tekrar oluşturulmaz).
    ///   Kart üzerindeki +/- ShoppingCart'ı günceller -> sağdaki sepet paneli senkronize olur.
    ///   Purchase: toplam tutar düşülür, HER satırın miktarı kadar AYRI sipariş oluşturulur.
    ///
    /// SAHNE KURULUMU:
    ///   categoryPanels: Inspector'dan her premade kategori panel/buton/konteyner elle atanır.
    ///   cartEntryParent: sağdaki sepet listesi
    /// </summary>
    public class ShopApp : AppWindow
    {
        [Serializable]
        private struct ShopCategoryPanel
        {
            [Tooltip("Bu panelde hangi kategoriye ait ürünlerin gösterileceği.")]
            public ShopCategoryDefinition category;

            [Tooltip("Sahnede önceden var olan kategori sekme butonu.")]
            public ShopCategoryButtonUI categoryButton;

            [Tooltip("Ürün kartlarının spawn edileceği, sahnede önceden var olan grid parent.")]
            public Transform itemContainer;

            [Tooltip("Sekme seçilince açılıp kapanacak panelin kök objesi (itemContainer'ın üst paneli).")]
            public GameObject panelRoot;
        }

        [SerializeField] private List<ShopItemDefinition> availableItems = new();
        [SerializeField] private List<ShopCategoryPanel> categoryPanels = new();
        [SerializeField] private ShopItemEntryUI shopItemEntryPrefab;

        [Header("Sepet Paneli")]
        [SerializeField] private Transform cartEntryParent;
        [SerializeField] private CartEntryUI cartEntryPrefab;
        [SerializeField] private TextMeshProUGUI totalCostLabel;
        [SerializeField] private Evo.UI.ProgressButton purchaseButton;

        [Header("Fiyat Renkleri")]
        [SerializeField] private Color affordableColor = Color.green;
        [SerializeField] private Color unaffordableColor = Color.red;

        private readonly ShoppingCart cart = new();

        protected override void OnOpened()
        {
            base.OnOpened();

            cart.OnCartChanged += RefreshCartPanel;

            PopulateAllCategoryPanels();
            WireCategoryButtons();
            SelectCategory(categoryPanels.Count > 0 ? categoryPanels[0].category : null);
            RefreshCartPanel();

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onComplete.AddListener(Checkout);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            cart.OnCartChanged -= RefreshCartPanel;
        }

        // ── Kategori Panelleri (premade) ─────────────────────────────

        /// <summary>Her panelin itemContainer'ını bir kez doldurur; sekme geçişlerinde tekrar spawn edilmez.</summary>
        private void PopulateAllCategoryPanels()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel.itemContainer == null) continue;

                FunctionLibrary.DestroyChildren(panel.itemContainer);

                var itemsInCategory = availableItems.Where(i => i.category == panel.category);
                foreach (var item in itemsInCategory)
                {
                    var entry = Instantiate(shopItemEntryPrefab, panel.itemContainer);
                    entry.Setup(item, cart);
                }
            }
        }

        private void WireCategoryButtons()
        {
            foreach (var panel in categoryPanels)
            {
                if (panel.categoryButton == null) continue;

                var capturedCategory = panel.category;
                panel.categoryButton.Setup(capturedCategory, () => SelectCategory(capturedCategory));
            }
        }

        private void SelectCategory(ShopCategoryDefinition category)
        {
            foreach (var panel in categoryPanels)
            {
                bool isSelected = panel.category == category;

                panel.categoryButton?.SetSelected(isSelected);
                if (panel.panelRoot != null) panel.panelRoot.SetActive(isSelected);
            }
        }

        // ── Sepet Paneli ─────────────────────────────────────────────

        private void RefreshCartPanel()
        {
            FunctionLibrary.DestroyChildren(cartEntryParent);

            int totalCost = cart.GetTotalCost();
            bool canAffordCart = CurrencyManager.Instance.GetMoney() >= totalCost;

            foreach (var pair in cart.Entries)
            {
                ShopItemDefinition item = pair.Key;
                int quantity = pair.Value;

                var entry = Instantiate(cartEntryPrefab, cartEntryParent);
                entry.Setup(item, quantity, canAffordCart,
                    onIncrement: () => cart.Increment(item),
                    onDecrement: () => cart.Decrement(item));
            }

            if (totalCostLabel != null)
            {
                totalCostLabel.text = $"${totalCost:F2}";
                totalCostLabel.color = canAffordCart ? affordableColor : unaffordableColor;
            }

            purchaseButton.interactable = !cart.IsEmpty && canAffordCart;
        }

        // ── Satın Alma ───────────────────────────────────────────────

        private void Checkout()
        {
            int totalCost = cart.GetTotalCost();
            if (cart.IsEmpty || CurrencyManager.Instance.GetMoney() < totalCost) return;

            CurrencyManager.Instance.SpendCurrency(totalCost);

            // Her ürün kendi CargoBox'ında ayrı teslim edilsin diye miktar kadar ayrı sipariş açılır.
            foreach (var pair in cart.Entries)
            {
                ShopItemDefinition item = pair.Key;
                int quantity = pair.Value;

                for (int i = 0; i < quantity; i++)
                    OrderDeliveryManager.Instance.CreateOrder(item);
            }

            cart.Clear();
        }
    }
}