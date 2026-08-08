using DG.Tweening;
using Evo.UI;
using UnityEngine;
using UnityEngine.Events;

namespace SyntaxSultan.UI
{
    public class ButtonBinder
    {
        private readonly Button button;
        private readonly RectTransform rectTransform;
        private readonly UnityAction onClick;

        public ButtonBinder(Button button, UnityAction onClick, bool useHoverPunch = true)
        {
            this.button = button;
            this.onClick = onClick;
            rectTransform = button.GetComponent<RectTransform>();

            button.onClick.AddListener(onClick);
            if (useHoverPunch)
                button.onPointerEnter.AddListener(OnHovered);
        }

        private void OnHovered()
        {
            FunctionLibrary.DoButtonPunch(rectTransform);
        }

        public void Unbind()
        {
            button.onClick.RemoveListener(onClick);
            button.onPointerEnter.RemoveAllListeners();
            if (rectTransform != null)
                rectTransform.DOKill();
        }
    }
}