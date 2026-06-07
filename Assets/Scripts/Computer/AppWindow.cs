using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SyntaxSultan.ComputerSystem
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]

    public class AppWindow : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private RectTransform titleBar;
        [SerializeField] private Image icon;
        [SerializeField] private Button closeButton;

        [Header("Animation")] [SerializeField] private float openDuration = 0.25f;
        [SerializeField] private float closeDuration = 0.15f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;

        [HideInInspector] public AppDefinition Definition;
        private WindowManager windowManager;

        protected RectTransform RectT { get; private set; }
        private CanvasGroup canvasGroup;
        private RectTransform parentRectT;
        private Camera canvasCamera;


        protected virtual void Awake()
        {
            RectT = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            parentRectT = RectT.parent as RectTransform;

            Canvas rootCanvas = GetComponentInParent<Canvas>();
            while (rootCanvas != null && !rootCanvas.isRootCanvas)
                rootCanvas = rootCanvas.transform.parent.GetComponentInParent<Canvas>();
            canvasCamera = rootCanvas != null ? rootCanvas.worldCamera : null;

            closeButton.onClick.AddListener(Close);
        }

        public void Setup(AppDefinition def, WindowManager wm)
        {
            Definition = def;
            windowManager = wm;
            if (icon) icon.sprite = def.icon;
            if (titleText) titleText.text = def.appName;
            OnOpened();
            PlayOpenAnimation();
        }

        public void Close()
        {
            closeButton.interactable = false;
            OnClosed();
            PlayCloseAnimation(() => windowManager.NotifyWindowClosed(this));
        }

        private void BringToFront() => transform.SetAsLastSibling();

        private void SetInteractable(bool value) => canvasGroup.interactable = value;

        public void OnPointerDown(PointerEventData eventData) => BringToFront();

        // ── Animasyon ────────────────────────────────────────────────

        private void PlayOpenAnimation()
        {
            SetInteractable(false);
            canvasGroup.alpha = 0f;
            RectT.localScale = Vector3.one * 0.85f;

            Sequence seq = DOTween.Sequence();
            seq.Join(RectT.DOScale(Vector3.one, openDuration).SetEase(openEase));
            seq.Join(canvasGroup.DOFade(1f, openDuration * 0.6f));
            seq.OnComplete(() => canvasGroup.interactable = true);
        }

        private void PlayCloseAnimation(Action onComplete)
        {
            SetInteractable(false);

            Sequence seq = DOTween.Sequence();
            seq.Join(RectT.DOScale(Vector3.one * 0.85f, closeDuration).SetEase(closeEase));
            seq.Join(canvasGroup.DOFade(0f, closeDuration));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnOpened()
        {
        }

        protected virtual void OnClosed()
        {
        }
    }
}