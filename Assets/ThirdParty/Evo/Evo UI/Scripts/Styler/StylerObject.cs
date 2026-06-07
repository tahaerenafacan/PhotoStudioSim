using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Evo.UI
{
    [DisallowMultipleComponent]
    [HelpURL(Constants.HELP_URL + "styler")]
    [AddComponentMenu("Evo/UI/Styler/Styler Object")]
    public class StylerObject : MonoBehaviour, IStylerHandler
    {
        [EvoHeader("References", Constants.CUSTOM_EDITOR_ID)]
        public StylerPreset preset;
        [UnityEngine.Serialization.FormerlySerializedAs("targetImage")]
        public Graphic targetGraphic;
        public TMP_Text targetText;

        [EvoHeader("Settings", Constants.CUSTOM_EDITOR_ID)]
        public ObjectType objectType = ObjectType.Graphic;
        public string colorID = "Primary";
        public string fontID = "Regular";
        public string spriteID = "";
        public bool useCustomColor = false;
        public bool overrideAlpha = false;
        [Range(0f, 1f)] public float alphaOverride = 1f;

        [EvoHeader("Interaction", Constants.CUSTOM_EDITOR_ID)]
        public bool enableInteraction = false;
        public Interactive interactableObject;
        public ColorMapping disabledColor = new() { stylerID = "Primary" };
        public ColorMapping normalColor = new() { stylerID = "Primary" };
        public ColorMapping highlightedColor = new() { stylerID = "Primary" };
        public ColorMapping pressedColor = new() { stylerID = "Primary" };
        public ColorMapping selectedColor = new() { stylerID = "Primary" };

        // Styler Interface
        public StylerPreset Preset
        {
            get => preset;
            set
            {
                if (preset == value) { return; }
                preset = value;
                UpdateStyler();
            }
        }

        // Cache
        readonly List<string> cachedColorIDs = new();
        readonly List<string> cachedFontIDs = new();
        readonly List<string> cachedSpriteIDs = new();

        // Interaction Cache
        InteractionState currentState;
        Coroutine tweenCoroutine;

        public enum ObjectType
        {
            [Tooltip("Sets the color of the Graphic variable.")]
            Graphic = 0,

            [Tooltip("Sets the font and color of the TMP variable.")]
            TMPText = 1,

            [Tooltip("Sets the sprite and color of the Image variable.")]
            [InspectorName("Image (Sprite)")] Image = 2
        }

        void Awake() => Styler.RegisteredObjects.Add(this);

        void OnEnable()
        {
            tweenCoroutine = null;

            // Subscribe to catch future states
            if (enableInteraction && interactableObject != null)
            {
                interactableObject.OnStateChanged += OnInteractableStateChanged;
                currentState = interactableObject.interactionState;
            }

            // Apply the visuals
            UpdateStyler();
        }

        void OnDisable()
        {
            // Unsubscribe to bypass animating hidden objects
            if (enableInteraction && interactableObject != null)
            {
                interactableObject.OnStateChanged -= OnInteractableStateChanged;
            }

            // Cleanup tweens
            if (tweenCoroutine != null)
            {
                StopCoroutine(tweenCoroutine);
                tweenCoroutine = null;

                if (currentState != InteractionState.Selected && currentState != InteractionState.Disabled)
                    currentState = InteractionState.Normal;
            }
        }

        void OnDestroy() => Styler.RegisteredObjects.Remove(this);

        void CheckComponents()
        {
            if ((objectType == ObjectType.Graphic || objectType == ObjectType.Image) && targetGraphic == null)
            {
                TryGetComponent(out targetGraphic);
                targetText = null;
            }
            else if (objectType == ObjectType.TMPText && targetText == null)
            {
                TryGetComponent(out targetText);
                targetGraphic = null;
            }
        }

        void OnInteractableStateChanged(InteractionState newState)
        {
            if (currentState == newState || !gameObject.activeInHierarchy)
                return;

            currentState = newState;
            AnimateToState(newState);
        }

        void AnimateToState(InteractionState state)
        {
            if (objectType == ObjectType.TMPText && targetText == null 
                || (objectType == ObjectType.Graphic || objectType == ObjectType.Image) && targetGraphic == null)
                return;

            float tDuration = Mathf.Max(0f, interactableObject != null ? interactableObject.transitionDuration : 0f);
            Color tColor = GetInteractionColor(state);
            Graphic tGraphic = objectType == ObjectType.Graphic || objectType == ObjectType.Image ? targetGraphic : targetText;

            if (tweenCoroutine != null) { StopCoroutine(tweenCoroutine); }
            tweenCoroutine = StartCoroutine(Utilities.CrossFadeGraphic(tGraphic, tColor, tDuration));
        }

        Color GetTargetColor()
        {
            if (enableInteraction && interactableObject != null)
                return GetInteractionColor(interactableObject.interactionState);

            if (string.IsNullOrEmpty(colorID) || preset == null)
                return Color.clear;

            Color baseColor = preset.GetColor(colorID);

            if (!useCustomColor && overrideAlpha)
                return new Color(baseColor.r, baseColor.g, baseColor.b, alphaOverride);

            return baseColor;
        }

        Color GetInteractionColor(InteractionState state)
        {
            ColorMapping mapping = GetColorMappingForState(state);

            // Use Styler Preset color
            if (!useCustomColor && preset != null)
            {
                if (string.IsNullOrEmpty(mapping.stylerID) || preset == null)
                    return Color.clear;

                Color baseColor = preset.GetColor(mapping.stylerID);

                if (!useCustomColor && overrideAlpha)
                    return new Color(baseColor.r, baseColor.g, baseColor.b, alphaOverride);

                return baseColor;
            }

            // Fallback to custom color
            Color customColor = mapping.color;

            if (preset != null && !useCustomColor && overrideAlpha)
                return new Color(customColor.r, customColor.g, customColor.b, alphaOverride);

            return customColor;
        }

        ColorMapping GetColorMappingForState(InteractionState state)
        {
            return state switch
            {
                InteractionState.Disabled => disabledColor,
                InteractionState.Normal => normalColor,
                InteractionState.Highlighted => highlightedColor,
                InteractionState.Pressed => pressedColor,
                InteractionState.Selected => selectedColor,
                _ => null
            };
        }

        void ApplySpriteSettings(Image targetImage, Styler.SpriteItem item)
        {
            if (targetImage == null)
                return;

            Sprite targetSprite = item?.spriteAsset;
            if (targetImage.sprite != targetSprite) { targetImage.sprite = targetSprite; }

            if (item != null && item.applyImageSettings)
            {
                if (targetImage.type != item.imageType) { targetImage.type = item.imageType; }
                if (targetImage.preserveAspect != item.preserveAspect) { targetImage.preserveAspect = item.preserveAspect; }
                if (targetImage.pixelsPerUnitMultiplier != item.pixelsPerUnitMultiplier) { targetImage.pixelsPerUnitMultiplier = item.pixelsPerUnitMultiplier; }

                if (item.imageType == Image.Type.Filled)
                {
                    if (targetImage.fillMethod != item.fillMethod) { targetImage.fillMethod = item.fillMethod; }
                    if (targetImage.fillOrigin != item.fillOrigin) { targetImage.fillOrigin = item.fillOrigin; }
                    if (targetImage.fillAmount != item.fillAmount) { targetImage.fillAmount = item.fillAmount; }
                    if (targetImage.fillClockwise != item.fillClockwise) { targetImage.fillClockwise = item.fillClockwise; }
                }
            }
        }

        public void UpdateStyler()
        {
            CheckComponents();

            if ((!enableInteraction && preset == null) || (targetGraphic == null && targetText == null))
                return;

            if (useCustomColor && !enableInteraction)
            {
                if (objectType == ObjectType.TMPText && targetText != null && preset != null)
                {
                    TMP_FontAsset targetFont = preset.GetFont(fontID);
                    if (targetText.font != targetFont) { targetText.font = targetFont; }
                }
                else if (objectType == ObjectType.Image && targetGraphic != null && preset != null)
                {
                    Image targetImage = targetGraphic as Image;
                    if (targetImage != null)
                    {
                        var spriteItem = preset.GetSpriteItem(spriteID);
                        ApplySpriteSettings(targetImage, spriteItem);
                    }
                }
                return;
            }

            Color targetColor = GetTargetColor();

            // Check for active transition
            if (tweenCoroutine != null)
            {
                StopCoroutine(tweenCoroutine);
                tweenCoroutine = null;
            }

            if (objectType == ObjectType.Graphic && targetGraphic.color != targetColor) { targetGraphic.color = targetColor; }
            else if (objectType == ObjectType.TMPText)
            {
                if (preset != null)
                {
                    TMP_FontAsset targetFont = preset.GetFont(fontID);
                    if (targetText.font != targetFont) { targetText.font = targetFont; }
                }

                if (targetText.color != targetColor)
                    targetText.color = targetColor;
            }
            else if (objectType == ObjectType.Image)
            {
                if (preset != null && targetGraphic != null)
                {
                    Image targetImage = targetGraphic as Image;
                    if (targetImage != null)
                    {
                        var spriteItem = preset.GetSpriteItem(spriteID);
                        ApplySpriteSettings(targetImage, spriteItem);
                    }
                }

                if (targetGraphic != null && targetGraphic.color != targetColor)
                    targetGraphic.color = targetColor;
            }
        }

        public List<string> GetAvailableColorIDs()
        {
            cachedColorIDs.Clear();

            if (preset == null)
                return cachedColorIDs;

            for (int i = 0; i < preset.colorItems.Count; i++)
                cachedColorIDs.Add(preset.colorItems[i].itemID);

            return cachedColorIDs;
        }

        public List<string> GetAvailableFontIDs()
        {
            cachedFontIDs.Clear();

            if (preset == null)
                return cachedFontIDs;

            for (int i = 0; i < preset.fontItems.Count; i++)
                cachedFontIDs.Add(preset.fontItems[i].itemID);

            return cachedFontIDs;
        }

        public List<string> GetAvailableSpriteIDs()
        {
            cachedSpriteIDs.Clear();

            if (preset == null)
                return cachedSpriteIDs;

            for (int i = 0; i < preset.spriteItems.Count; i++)
                cachedSpriteIDs.Add(preset.spriteItems[i].itemID);

            return cachedSpriteIDs;
        }

        #region Obsolete
        [System.Obsolete("Use UpdateStyler() instead. This method will be removed in future versions.")]
        public void UpdateStyle() => UpdateStyler();

        [System.Obsolete("Use targetGraphic instead. This method will be removed in future versions.")]
        public Image targetImage
        {
            get => targetGraphic as Image;
            set => targetGraphic = value;
        }
        #endregion

#if UNITY_EDITOR
        [HideInInspector] public bool referencesFoldout = true;
        [HideInInspector] public bool settingsFoldout = true;
        [HideInInspector] public bool interactionFoldout = true;

        void Reset()
        {
            CheckComponents();
            if (preset == null) { preset = Styler.GetDefaultPreset(false); }
        }

        void OnValidate()
        {
            if (!this.enabled)
                return;

            if (preset != null)
            {
                var availableColors = GetAvailableColorIDs();
                var availableFonts = GetAvailableFontIDs();
                var availableSprites = GetAvailableSpriteIDs();

                if (!string.IsNullOrEmpty(colorID) && !availableColors.Contains(colorID) && availableColors.Count > 0) { colorID = availableColors[0]; }
                if (!string.IsNullOrEmpty(fontID) && !availableFonts.Contains(fontID) && availableFonts.Count > 0) { fontID = availableFonts[0]; }
                if (!string.IsNullOrEmpty(spriteID) && !availableSprites.Contains(spriteID) && availableSprites.Count > 0) { spriteID = availableSprites[0]; }
            }

            UpdateStyler();
        }
#endif
    }
}