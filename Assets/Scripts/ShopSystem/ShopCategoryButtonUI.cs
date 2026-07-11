using System;
using UnityEngine;
using UnityEngine.UI;
using SyntaxSultan.ShopSystem;

namespace SyntaxSultan.ComputerSystem.Apps
{
    public class ShopCategoryButtonUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedHighlight;
        [SerializeField] private Evo.UI.Button button;

        private Action onClicked;

        public void Setup(ShopCategoryDefinition category, Action onClick)
        {
            if (iconImage != null) iconImage.sprite = category.icon;
            onClicked = onClick;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke());

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null) selectedHighlight.SetActive(selected);
        }
    }
}