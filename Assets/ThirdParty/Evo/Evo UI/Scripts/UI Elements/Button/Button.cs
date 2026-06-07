using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

namespace Evo.UI
{
    [DisallowMultipleComponent]
    [HelpURL(Constants.HELP_URL + "ui-elements/button")]
    [AddComponentMenu("Evo/UI/UI Elements/Button")]
    public class Button : Interactive
    {
        [EvoHeader("Icon", Constants.CUSTOM_EDITOR_ID)]
        public bool enableIcon = true;
        public Sprite icon;
        public LocalizedSprite localizedIcon;
        [Range(1, 200)] public float iconSize = 30;

        [EvoHeader("Text", Constants.CUSTOM_EDITOR_ID)]
        public bool enableText = true;
        public string text = "Button";
        public LocalizedString localizedText;
        [Range(1, 200)] public float textSize = 24;

        [EvoHeader("Layout", Constants.CUSTOM_EDITOR_ID)]
        public bool dynamicScale = true;
        public bool reverseArrangement = false;
        [Range(0, 100)] public int spacing = 12;
        public RectOffset padding = new();

#if EVO_LOCALIZATION
        [EvoHeader("Localization", Constants.CUSTOM_EDITOR_ID)]
        public bool enableLocalization = true;
        public Localization.LocalizedObject localizedObject;
#endif

        [EvoHeader("Settings", Constants.CUSTOM_EDITOR_ID)]
        public bool customContent = false;
        public bool allowDoubleClick = false;
        [SerializeField, Range(0.1f, 1)] private float doubleClickDuration = 0.25f;

        [EvoHeader("References", Constants.CUSTOM_EDITOR_ID)]
        public Image imageObject;
        [SerializeField] private LayoutElement iconElement;
        public TMP_Text textObject;
        [SerializeField] private RectTransform textContainer;
        [SerializeField] private ContentSizeFitter contentFitter;
        [SerializeField] private HorizontalLayoutGroup contentLayout;

        // Cache
        Coroutine doubleClickCoroutine;

        protected override void Awake()
        {
            base.Awake();
            UpdateUI();
        }

#if EVO_LOCALIZATION
        protected override void Start()
        {
            base.Start();
            if (Application.isPlaying && enableLocalization && localizedObject == null && !customContent)
            {
                localizedObject = Localization.LocalizedObject.Check(gameObject, textObject);
            }
        }
#endif

        protected override void Start()
        {
            base.Start();
            localizedText.StringChanged += (_) => UpdateUI();
            localizedIcon.AssetChanged += (_) => UpdateUI();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (IsInteractable() && allowDoubleClick)
            {
                if (waitingForDoubleClickInput)
                {
                    onDoubleClick?.Invoke();
                    waitingForDoubleClickInput = false;

                    if (doubleClickCoroutine != null)
                    {
                        StopCoroutine(doubleClickCoroutine);
                        doubleClickCoroutine = null;
                    }
                }
                else
                {
                    waitingForDoubleClickInput = true;

                    if (doubleClickCoroutine != null) { StopCoroutine(doubleClickCoroutine); }
                    doubleClickCoroutine = StartCoroutine(DoubleClickTimer());
                }
            }
        }

        IEnumerator DoubleClickTimer()
        {
            float elapsed = 0f;
            while (elapsed < doubleClickDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            waitingForDoubleClickInput = false;
            doubleClickCoroutine = null;
        }

        public void UpdateUI()
        {
            SetIcon(icon);
            SetText(text);
            UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (contentFitter != null)
                contentFitter.enabled = dynamicScale;

            if (contentLayout != null)
            {
                contentLayout.childForceExpandHeight = dynamicScale;
                contentLayout.childForceExpandWidth = dynamicScale;
                contentLayout.padding = padding;
                contentLayout.spacing = spacing;
                contentLayout.reverseArrangement = reverseArrangement;
            }
        }

        public void SetIcon(Sprite newIcon)
        {
            icon = newIcon;

            if (customContent || imageObject == null)
                return;

            Sprite displayIcon = icon;

            // Localized Icon kontrolü
            if (localizedIcon != null && !localizedIcon.IsEmpty)
            {
                var loadedIcon = localizedIcon.LoadAssetAsync().WaitForCompletion();
                if (loadedIcon != null)
                {
                    displayIcon = loadedIcon;
                }
            }

            imageObject.gameObject.SetActive(enableIcon && displayIcon);

            if (enableIcon)
            {
                imageObject.sprite = displayIcon;
                if (iconElement != null)
                {
                    iconElement.preferredWidth = iconSize;
                    iconElement.preferredHeight = iconSize;
                }
            }
        }

        public void SetText(string newText)
        {
            text = newText;

            if (customContent || textObject == null)
                return;

            bool bypassText = false;
#if EVO_LOCALIZATION
            bypassText = enableLocalization && localizedObject != null && !string.IsNullOrEmpty(localizedObject.tableKey);
#endif

            textObject.gameObject.SetActive(enableText);

            if (textContainer != null)
                textContainer.gameObject.SetActive(enableText);

            if (enableText)
            {
                if (!bypassText)
                {
                    string displayText = text;

                    // Localized String kontrolü
                    if (localizedText != null && !localizedText.IsEmpty)
                    {
                        string locText = localizedText.GetLocalizedString();
                        if (!string.IsNullOrEmpty(locText))
                        {
                            displayText = locText;
                        }
                    }

                    textObject.text = displayText;
                }
                textObject.fontSize = textSize;
            }
        }

#if UNITY_EDITOR
        [HideInInspector] public bool objectFoldout = true;

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateUI();
        }
#endif
    }
}