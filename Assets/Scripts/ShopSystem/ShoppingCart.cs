using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxSultan.ShopSystem
{
    /// <summary>
    /// Sepetteki ürün/miktar verisinin tek doğruluk kaynağı (SSOT).
    /// MonoBehaviour değildir; ShopApp bir instance oluşturup UI ile senkronize eder.
    /// </summary>
    public class ShoppingCart
    {
        private readonly Dictionary<ShopItemDefinition, int> quantities = new();

        /// <summary>Sepet her değiştiğinde tetiklenir; UI bunu dinleyip kendini yeniler.</summary>
        public event Action OnCartChanged;

        public IReadOnlyDictionary<ShopItemDefinition, int> Entries => quantities;
        public bool IsEmpty => quantities.Count == 0;

        /// <summary>Ürün miktarını değiştirir. 0 veya altına düşerse sepetten kaldırılır.</summary>
        public void SetQuantity(ShopItemDefinition item, int quantity)
        {
            if (quantity <= 0)
            {
                if (quantities.Remove(item))
                    OnCartChanged?.Invoke();
                return;
            }

            quantities[item] = quantity;
            OnCartChanged?.Invoke();
        }

        public int GetQuantity(ShopItemDefinition item) =>
            quantities.TryGetValue(item, out int qty) ? qty : 0;

        public void Increment(ShopItemDefinition item) => SetQuantity(item, GetQuantity(item) + 1);
        public void Decrement(ShopItemDefinition item) => SetQuantity(item, GetQuantity(item) - 1);

        public int GetTotalCost() => quantities.Sum(kvp => kvp.Key.price * kvp.Value);

        public void Clear()
        {
            quantities.Clear();
            OnCartChanged?.Invoke();
        }
    }
}