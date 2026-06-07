namespace Evo.UI
{
    public static class Constants
    {
        /// <summary>
        /// Used as a key to set and retrieve the custom editor ID.
        /// </summary>
        public const string CUSTOM_EDITOR_ID = "Evo_UI";

        /// <summary>
        /// Default Styler preset asset root and name.
        /// </summary>
        public const string STYLER_FALLBACK_PATH = "Styler Presets/Default";

        /// <summary>
        /// Styler config asset root and name. Used for fetching default preset.
        /// </summary>
        public const string STYLER_CONFIG_PATH = "Styler Presets/Config";

        /// <summary>
        /// Default Styler preset file root and name.
        /// </summary>
        public const string DEFAULT_ICON_LIBRARY = "Icon Library/Default";

        /// <summary>
        /// Script define symbol to identify the package and run specific methods.
        /// Intended for integrations.
        /// </summary>
        public const string DEFINE_SYMBOL = "EVO_UI";

        /// <summary>
        /// For inspector shortcut.
        /// </summary>
        public const string HELP_URL = "https://evo.michsky.com/docs/evo-ui/";
    }
}