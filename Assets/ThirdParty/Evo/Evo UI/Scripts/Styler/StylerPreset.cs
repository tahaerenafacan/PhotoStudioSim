using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Evo.UI
{
    [HelpURL(Constants.HELP_URL + "styler")]
    [CreateAssetMenu(fileName = "Styler Preset", menuName = "Evo/UI/Styler Preset")]
    public class StylerPreset : ScriptableObject
    {
        [EvoHeader("Audio", Constants.CUSTOM_EDITOR_ID)]
        public List<Styler.AudioItem> audioItems = new()
        {
            new("Hover SFX", null),
            new("Click SFX", null),
            new("Notification SFX", null)
        };

        [EvoHeader("Color", Constants.CUSTOM_EDITOR_ID)]
        public List<Styler.ColorItem> colorItems = new()
        {
            new Styler.ColorItem("Primary", Color.white),
            new Styler.ColorItem("Secondary", new Color(0.1f, 0.15f, 0.2f, 1f))
        };

        [EvoHeader("Font", Constants.CUSTOM_EDITOR_ID)]
        public List<Styler.FontItem> fontItems = new()
        {
            new Styler.FontItem("Thin", null),
            new Styler.FontItem("Light", null),
            new Styler.FontItem("Regular", null),
            new Styler.FontItem("Semibold", null),
            new Styler.FontItem("Bold", null)
        };

        [EvoHeader("Sprite", Constants.CUSTOM_EDITOR_ID)]
        public List<Styler.SpriteItem> spriteItems = new() { };

        [EvoHeader("Settings", Constants.CUSTOM_EDITOR_ID)]
        public Styler.UpdateMode updateMode = Styler.UpdateMode.Adaptive;

        /// <summary>
        /// Get audio clip from the preset.
        /// </summary>
        public AudioClip GetAudio(string itemID)
        {
            for (int i = 0; i < audioItems.Count; i++)
            {
                if (audioItems[i].itemID == itemID)
                    return audioItems[i].audioAsset;
            }

            return null;
        }

        /// <summary>
        /// Get color from the preset.
        /// </summary>
        public Color GetColor(string itemID)
        {
            for (int i = 0; i < colorItems.Count; i++)
            {
                if (colorItems[i].itemID == itemID)
                    return colorItems[i].colorValue;
            }

            return Color.white;
        }

        /// <summary>
        /// Get font from the preset.
        /// </summary>
        public TMP_FontAsset GetFont(string itemID)
        {
            for (int i = 0; i < fontItems.Count; i++)
            {
                if (fontItems[i].itemID == itemID)
                    return fontItems[i].fontAsset;
            }

            return null;
        }

        /// <summary>
        /// Get sprite from the preset.
        /// </summary>
        public Sprite GetSprite(string itemID)
        {
            for (int i = 0; i < spriteItems.Count; i++)
            {
                if (spriteItems[i].itemID == itemID)
                    return spriteItems[i].spriteAsset;
            }

            return null;
        }

        /// <summary>
        /// Get the full sprite item definition from the preset.
        /// </summary>
        public Styler.SpriteItem GetSpriteItem(string itemID)
        {
            for (int i = 0; i < spriteItems.Count; i++)
            {
                if (spriteItems[i].itemID == itemID)
                    return spriteItems[i];
            }

            return null;
        }

        /// <summary>
        /// Set audio clip for an existing item or add new if it doesn't exist.
        /// </summary>
        public void SetAudio(string itemID, AudioClip audioClip)
        {
            for (int i = 0; i < audioItems.Count; i++)
            {
                if (audioItems[i].itemID == itemID)
                {
                    audioItems[i].audioAsset = audioClip;
                    return;
                }
            }

            audioItems.Add(new Styler.AudioItem(itemID, audioClip));
        }

        /// <summary>
        /// Set color for an existing item or add new if it doesn't exist.
        /// </summary>
        public void SetColor(string itemID, Color color)
        {
            for (int i = 0; i < colorItems.Count; i++)
            {
                if (colorItems[i].itemID == itemID)
                {
                    colorItems[i].colorValue = color;
                    return;
                }
            }

            colorItems.Add(new Styler.ColorItem(itemID, color));
        }

        /// <summary>
        /// Set font for an existing item or add new if it doesn't exist.
        /// </summary>
        public void SetFont(string itemID, TMP_FontAsset font)
        {
            for (int i = 0; i < fontItems.Count; i++)
            {
                if (fontItems[i].itemID == itemID)
                {
                    fontItems[i].fontAsset = font;
                    return;
                }
            }

            fontItems.Add(new Styler.FontItem(itemID, font));
        }

        /// <summary>
        /// Set sprite for an existing item or add new if it doesn't exist.
        /// </summary>
        public void SetSprite(string itemID, Sprite sprite)
        {
            for (int i = 0; i < spriteItems.Count; i++)
            {
                if (spriteItems[i].itemID == itemID)
                {
                    spriteItems[i].spriteAsset = sprite;
                    return;
                }
            }

            spriteItems.Add(new Styler.SpriteItem(itemID, sprite));
        }

        /// <summary>
        /// Add an audio item to the preset.
        /// </summary>
        public void AddAudio(string itemID, AudioClip audioClip)
        {
            for (int i = 0; i < audioItems.Count; i++)
            {
                if (audioItems[i].itemID == itemID)
                {
                    Debug.LogWarning($"Audio item with ID '{itemID}' already exists.", this);
                    return;
                }
            }

            audioItems.Add(new Styler.AudioItem(itemID, audioClip));
        }

        /// <summary>
        /// Add a color item to the preset.
        /// </summary>
        public void AddColor(string itemID, Color color)
        {
            for (int i = 0; i < colorItems.Count; i++)
            {
                if (colorItems[i].itemID == itemID)
                {
                    Debug.LogWarning($"Color item with ID '{itemID}' already exists.", this);
                    return;
                }
            }

            colorItems.Add(new Styler.ColorItem(itemID, color));
        }

        /// <summary>
        /// Add a font item to the preset.
        /// </summary>
        public void AddFont(string itemID, TMP_FontAsset font)
        {
            for (int i = 0; i < fontItems.Count; i++)
            {
                if (fontItems[i].itemID == itemID)
                {
                    Debug.LogWarning($"Font item with ID '{itemID}' already exists.", this);
                    return;
                }
            }

            fontItems.Add(new Styler.FontItem(itemID, font));
        }

        /// <summary>
        /// Add a sprite item to the preset.
        /// </summary>
        public void AddSprite(string itemID, Sprite sprite)
        {
            for (int i = 0; i < spriteItems.Count; i++)
            {
                if (spriteItems[i].itemID == itemID)
                {
                    Debug.LogWarning($"Sprite item with ID '{itemID}' already exists.", this);
                    return;
                }
            }

            spriteItems.Add(new Styler.SpriteItem(itemID, sprite));
        }

        /// <summary>
        /// Remove an audio item from the preset.
        /// </summary>
        public bool RemoveAudio(string itemID)
        {
            for (int i = 0; i < audioItems.Count; i++)
            {
                if (audioItems[i].itemID == itemID)
                {
                    audioItems.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a color item from the preset.
        /// </summary>
        public bool RemoveColor(string itemID)
        {
            for (int i = 0; i < colorItems.Count; i++)
            {
                if (colorItems[i].itemID == itemID)
                {
                    colorItems.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a font item from the preset.
        /// </summary>
        public bool RemoveFont(string itemID)
        {
            for (int i = 0; i < fontItems.Count; i++)
            {
                if (fontItems[i].itemID == itemID)
                {
                    fontItems.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a sprite item from the preset.
        /// </summary>
        public bool RemoveSprite(string itemID)
        {
            for (int i = 0; i < spriteItems.Count; i++)
            {
                if (spriteItems[i].itemID == itemID)
                {
                    spriteItems.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        [HideInInspector] public bool audioFoldout = false;
        [HideInInspector] public bool colorFoldout = false;
        [HideInInspector] public bool fontFoldout = false;
        [HideInInspector] public bool spriteFoldout = false;
        [HideInInspector] public bool settingsFoldout = false;

        void OnValidate()
        {
            if (updateMode == Styler.UpdateMode.Always || (!Application.isPlaying && updateMode == Styler.UpdateMode.Adaptive))
            {
                NotifyStylerObjects();
            }
        }

        void NotifyStylerObjects()
        {
            // Find all MonoBehaviours, but filter for the ones implementing our interface
            var targets = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is IStylerHandler handler && handler.Preset == this)
                    handler.UpdateStyler();
            }
        }
#endif
    }
}