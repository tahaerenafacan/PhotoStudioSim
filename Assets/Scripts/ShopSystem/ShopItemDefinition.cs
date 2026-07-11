using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ShopSystem
{
    /// <summary>
    /// Mağazada satılan bir ürünün verisi.
    /// Ad/ikon/prefab tekrarlanmaz; SSOT için mevcut ItemDefinition'a referans verilir.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopItem", menuName = "PSSGame/Shop/Shop Item Definition")]
    public class ShopItemDefinition : ScriptableObject
    {
        [Header("Referans Item")]
        [Tooltip("Ad, ikon ve spawn prefabı buradan okunur (ItemSystem/ItemDefinition.cs).")]
        public ItemDefinition itemDefinition;


        [Header("Kategori")]
        [Tooltip("Sol sekmede bu ürünün hangi kategori altında listeleneceği.")]
        public ShopCategoryDefinition category;

        [Header("Mağaza Ayarları")]
        public int price;

        [Tooltip("Satın alımdan sonra kargonun dolaba düşmesi için geçecek süre (saniye).")]
        public float deliveryTimeSeconds = 60f;

        public LocalizedString ItemName => itemDefinition.itemName;
        public Sprite Icon => itemDefinition.icon;
        public BasePickableItem ItemPrefab => itemDefinition.itemPrefab;
    }
}