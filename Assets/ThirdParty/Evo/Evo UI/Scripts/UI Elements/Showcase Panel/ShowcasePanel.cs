using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Localization;
using System;

namespace Evo.UI
{
    [DisallowMultipleComponent]
    [HelpURL(Constants.HELP_URL + "ui-elements/showcase-panel")]
    [AddComponentMenu("Evo/UI/UI Elements/Showcase Panel")]
    public class ShowcasePanel : MonoBehaviour
    {
        [EvoHeader("Content", Constants.CUSTOM_EDITOR_ID)]
        public int currentIndex;
        public List<Item> items = new();

        [EvoHeader("Settings", Constants.CUSTOM_EDITOR_ID)]
        public bool useUnscaledTime = false;
        public bool setWithTimer = true;
        [Range(1f, 30f)] public float timer = 3;
        [Range(0.05f, 2f)] public float animationDuration = 0.25f;
        public Vector2 slideOffset = new(0, 15);

        [EvoHeader("Unity Localization", Constants.CUSTOM_EDITOR_ID)]
        public bool enableLocalization = true;

        [EvoHeader("References", Constants.CUSTOM_EDITOR_ID)]
        [SerializeField] private Transform buttonParent;
        [SerializeField] private GameObject buttonPreset;
        [SerializeField] private TextMeshProUGUI textDisplay;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image backgroundShadow;

        // Helpers
        int displayedIndex = -1;
        float timerCount = 0;
        bool isInitialized;
        bool updateTimer;
        bool isTransitionInProgress;

        // Cache
        CanvasGroup textCanvasGroup;
        CanvasGroup backgroundCanvasGroup;
        Coroutine hoverCoroutine;
        Coroutine shadowCoroutine;
        WaitForSeconds cachedHoverWait;
        WaitForSecondsRealtime cachedHoverWaitRealtime;

        private Dictionary<Item, LocalizedString.ChangeHandler> titleDelegates = new();
        private Dictionary<Item, LocalizedString.ChangeHandler> descDelegates = new();

        // Constants
        const float HoverTransitionDelay = 0.15f;
        const float AnimationSplit = 0.5f;

        [System.Serializable]
        public class Item
        {
            public string title = "Item Title";
            public string url;
            public Sprite icon;
            public Sprite background;
            public Color shadowColor = Color.clear;
            [TextArea(2, 4)] public string description = "Item description";
            public UnityEvent onClick = new();
            [HideInInspector] public Button button;

            [Header("Unity Localization")]
            public LocalizedString localizedTitle;
            public LocalizedString localizedDescription;
        }

        void OnEnable()
        {
            if (!isInitialized) { Initialize(); }
            else { ShowCurrentItem(); }

            if (enableLocalization) SubscribeLocalization();
        }

        void OnDisable()
        {
            if (enableLocalization) UnsubscribeLocalization();
        }

        void Update()
        {
            if (!isInitialized || !updateTimer || !setWithTimer)
                return;

            timerCount += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (timerCount > timer)
            {
                if (items.Count > 1)
                    StartCoroutine(DoTimerTransition());

                timerCount = 0;
            }
        }

        public void Initialize()
        {
            cachedHoverWait = new WaitForSeconds(HoverTransitionDelay);
            cachedHoverWaitRealtime = new WaitForSecondsRealtime(HoverTransitionDelay);

            // Clean up existing buttons
            foreach (Transform child in buttonParent)
                Destroy(child.gameObject);

            SetupCanvasGroups();
            CreateButtons();
            ShowCurrentItem();

            isInitialized = true;
        }

        void SetupCanvasGroups()
        {
            if (textDisplay != null)
            {
                if (!textDisplay.TryGetComponent(out textCanvasGroup))
                    textCanvasGroup = textDisplay.gameObject.AddComponent<CanvasGroup>();
            }

            if (backgroundImage != null)
            {
                if (!backgroundImage.TryGetComponent(out backgroundCanvasGroup))
                    backgroundCanvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        void CreateButtons()
        {
            for (int i = 0; i < items.Count; ++i)
            {
                int tempIndex = i; // Capture for closure

                GameObject btnGO = Instantiate(buttonPreset, buttonParent);
                btnGO.name = items[tempIndex].title;

                Button btn = btnGO.GetComponent<Button>();
                items[tempIndex].button = btn;

                btn.SetIcon(items[tempIndex].icon);
                btn.SetText(items[tempIndex].title);

                // Setup click event
                btn.onClick.AddListener(() => HandleButtonClick(tempIndex));

                // Setup hover events
                btn.onPointerEnter.AddListener(() => HandleButtonHover(tempIndex, btn));
                btn.onPointerExit.AddListener(() => HandleButtonLeave());
            }
        }

        void HandleButtonClick(int index)
        {
            items[index].onClick.Invoke();

            if (!string.IsNullOrEmpty(items[index].url))
                Application.OpenURL(items[index].url);
        }

        void HandleButtonHover(int index, Button hoveredButton)
        {
            SetButtonsInteractable(false, hoveredButton);

            if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
            hoverCoroutine = StartCoroutine(SetItemByHover(index));
        }

        void HandleButtonLeave()
        {
            updateTimer = true;
            SetButtonsInteractable(true);
            HighlightCurrentButton();
        }

        void SetButtonsInteractable(bool interactable, Button except = null)
        {
            for (int x = 0; x < items.Count; ++x)
            {
                if (items[x].button != except && items[x].button.interactable != interactable)
                    items[x].button.SetInteractable(interactable);
            }
        }

        void HighlightCurrentButton()
        {
            if (currentIndex >= 0 && currentIndex < items.Count)
            {
                items[currentIndex].button.SetState(InteractionState.Highlighted);
            }
        }

        void ShowCurrentItem()
        {
            SetItemContent(currentIndex);
            HighlightCurrentButton();
            StartCoroutine(PlayInAnimation());

            if (backgroundShadow != null && currentIndex >= 0 && currentIndex < items.Count)
            {
                if (backgroundShadow.color != items[currentIndex].shadowColor)
                    backgroundShadow.color = items[currentIndex].shadowColor;
            }

            displayedIndex = currentIndex;
            timerCount = 0;
            updateTimer = true;
        }

        void SetItemContent(int index)
        {
            Item item = items[index];

            if (textDisplay != null && textDisplay.text != item.description)
                textDisplay.text = item.description;

            if (backgroundImage != null)
            {
                bool hasBackground = item.background != null;

                if (backgroundImage.gameObject.activeSelf != hasBackground) { backgroundImage.gameObject.SetActive(hasBackground); }
                if (hasBackground && backgroundImage.sprite != item.background) { backgroundImage.sprite = item.background; }
            }
        }

        void AnimateText(Vector2 startPos, Vector2 endPos, float startAlpha, float endAlpha, float progress)
        {
            if (textDisplay != null)
            {
                textDisplay.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
                textCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            }
        }

        void AnimateBackground(float startAlpha, float endAlpha, float progress)
        {
            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            }
        }

        void FinalizeTextAnimation(Vector2 position, float alpha)
        {
            if (textDisplay != null)
            {
                textDisplay.rectTransform.anchoredPosition = position;
                textCanvasGroup.alpha = alpha;
            }
        }

        void FinalizeBackgroundAnimation(float alpha)
        {
            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = alpha;
            }
        }

        bool ShouldAnimateBackground(int index)
        {
            return backgroundImage != null &&
                   index >= 0 &&
                   index < items.Count &&
                   items[index].background != null;
        }

        IEnumerator DoTimerTransition()
        {
            items[currentIndex].button.SetState(InteractionState.Normal);
            yield return StartCoroutine(PlayOutAnimation());

            currentIndex = (currentIndex + 1) % items.Count;

            SetItemContent(currentIndex);
            displayedIndex = currentIndex;
            HighlightCurrentButton();

            if (backgroundShadow != null)
            {
                Color tsColor = items[currentIndex].shadowColor;
                if (backgroundShadow.color != tsColor)
                {
                    if (shadowCoroutine != null) { StopCoroutine(shadowCoroutine); }
                    shadowCoroutine = StartCoroutine(Utilities.CrossFadeGraphic(backgroundShadow, tsColor, animationDuration));
                }
            }

            yield return StartCoroutine(PlayInAnimation());
        }

        IEnumerator SetItemByHover(int index)
        {
            updateTimer = false;
            timerCount = 0;
            currentIndex = index;

            if (displayedIndex == index && !isTransitionInProgress)
                yield break;

            if (isTransitionInProgress)
                yield return useUnscaledTime ? cachedHoverWaitRealtime : cachedHoverWait;

            isTransitionInProgress = true;

            yield return StartCoroutine(PlayOutAnimation());

            SetItemContent(currentIndex);
            displayedIndex = index;

            if (backgroundShadow != null)
            {
                Color tsColor = items[currentIndex].shadowColor;
                if (backgroundShadow.color != tsColor)
                {
                    if (shadowCoroutine != null) { StopCoroutine(shadowCoroutine); }
                    shadowCoroutine = StartCoroutine(Utilities.CrossFadeGraphic(backgroundShadow, tsColor, animationDuration));
                }
            }

            yield return StartCoroutine(PlayInAnimation());
            isTransitionInProgress = false;
        }

        IEnumerator PlayOutAnimation()
        {
            float elapsed = 0f;
            float duration = animationDuration * AnimationSplit;

            Vector2 textStartPos = Vector2.zero;
            float textStartAlpha = 1f;
            float bgStartAlpha = 1f;
            bool shouldAnimateBackground = ShouldAnimateBackground(displayedIndex);

            if (textDisplay != null)
            {
                textStartPos = textDisplay.rectTransform.anchoredPosition;
                textStartAlpha = textCanvasGroup.alpha;
            }

            if (shouldAnimateBackground)
                bgStartAlpha = backgroundCanvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float progress = elapsed / duration;

                AnimateText(textStartPos, -slideOffset, textStartAlpha, 0f, progress);

                if (shouldAnimateBackground)
                    AnimateBackground(bgStartAlpha, 0f, progress);

                yield return null;
            }

            FinalizeTextAnimation(-slideOffset, 0f);
            if (shouldAnimateBackground) { FinalizeBackgroundAnimation(0f); }
        }

        IEnumerator PlayInAnimation()
        {
            float elapsed = 0f;
            float duration = animationDuration * AnimationSplit;
            bool shouldAnimateBackground = ShouldAnimateBackground(currentIndex);

            if (shouldAnimateBackground)
                backgroundCanvasGroup.alpha = 0f;

            if (textDisplay != null)
            {
                textDisplay.rectTransform.anchoredPosition = slideOffset;
                textCanvasGroup.alpha = 0f;
            }

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float progress = elapsed / duration;

                AnimateText(slideOffset, Vector2.zero, 0f, 1f, progress);

                if (shouldAnimateBackground)
                    AnimateBackground(0f, 1f, progress);

                yield return null;
            }

            FinalizeTextAnimation(Vector2.zero, 1f);
            if (shouldAnimateBackground) { FinalizeBackgroundAnimation(1f); }
        }

        #region Unity Localization Integration

        void SubscribeLocalization()
        {
            foreach (Item item in items)
            {
                if (!titleDelegates.ContainsKey(item))
                {
                    titleDelegates[item] = (string value) =>
                    {
                        item.title = value;
                        if (item.button != null) item.button.SetText(value);
                    };
                }

                if (item.localizedTitle != null && !item.localizedTitle.IsEmpty)
                {
                    item.localizedTitle.StringChanged += titleDelegates[item];
                }

                // Description için delegeyi oluştur ve önbelleğe al
                if (!descDelegates.ContainsKey(item))
                {
                    descDelegates[item] = (string value) =>
                    {
                        item.description = value;
                        if (items.IndexOf(item) == currentIndex && textDisplay != null)
                            textDisplay.text = value;
                    };
                }

                if (item.localizedDescription != null && !item.localizedDescription.IsEmpty)
                {
                    item.localizedDescription.StringChanged += descDelegates[item];
                }
            }
        }

        void UnsubscribeLocalization()
        {
            foreach (Item item in items)
            {
                if (titleDelegates.ContainsKey(item) && item.localizedTitle != null && !item.localizedTitle.IsEmpty)
                    item.localizedTitle.StringChanged -= titleDelegates[item];

                if (descDelegates.ContainsKey(item) && item.localizedDescription != null && !item.localizedDescription.IsEmpty)
                    item.localizedDescription.StringChanged -= descDelegates[item];
            }
        }

        #endregion

#if UNITY_EDITOR
        [HideInInspector] public bool objectFoldout = true;
        [HideInInspector] public bool settingsFoldout = true;
        [HideInInspector] public bool referencesFoldout = false;
#endif
    }
}