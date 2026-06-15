using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Dosya veya klasörü temsil eden tek UI satırı/ikonu.
    /// FileManagerWindow tarafından runtime'da Instantiate edilir.
    /// Tek tık: seçim; çift tık: aç/navigate.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class FileSystemEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Image           iconImage;
        [SerializeField] private Image           selectionHighlight;
        [SerializeField] private LayoutElement   indentLayout;
        [SerializeField] private LayoutElement   iconSpacer;

        private Button button;
        private float  lastClickTime;
        private const float DoubleClickThreshold = 0.35f;

        private Action onSingleClick;
        private Action onDoubleClick;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
            if (selectionHighlight) selectionHighlight.enabled = false;
        }

        public void Setup(string name, Sprite icon, Action onDoubleClickAction, int indentDepth = 0, Action onSingleClickAction = null)
        {
            if (nameLabel)    nameLabel.text    = name;
            if (icon != null && iconImage) iconImage.sprite = icon;
            if (indentLayout) indentLayout.minWidth = indentDepth * 14f;
            if (iconSpacer) iconSpacer.minWidth     = indentDepth * 14f;
            Debug.Log(indentDepth);
            
            onSingleClick = onSingleClickAction;
            onDoubleClick = onDoubleClickAction;
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight) selectionHighlight.enabled = selected;
        }

        private void HandleClick()
        {
            float now = Time.unscaledTime;
            if (now - lastClickTime <= DoubleClickThreshold)
                onDoubleClick?.Invoke();
            else
                onSingleClick?.Invoke();
            lastClickTime = now;
        }
    }
}