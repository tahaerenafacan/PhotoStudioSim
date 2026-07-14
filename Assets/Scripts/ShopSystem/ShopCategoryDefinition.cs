using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ShopSystem
{
    [CreateAssetMenu(fileName = "NewShopCategory", menuName = "PSSGame/Shop/Shop Category Definition")]
    public class ShopCategoryDefinition : ScriptableObject
    {
        public LocalizedString categoryName;
        public Sprite icon;
    }
}