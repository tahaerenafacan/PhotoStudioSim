using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SyntaxSultan.ComputerSystem
{
    [RequireComponent(typeof(Button))]

    public class DesktopIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button button;

        private AppDefinition def;
        private AppManager appManager;

        // Çift tıklama takibi
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        public void Setup(AppDefinition appDef, AppManager inAppManager)
        {
            def = appDef;
            appManager = inAppManager;

            if (iconImage && def.icon) iconImage.sprite = def.icon;
            if (label) label.text = def.appName;
            if (button) button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            float now = Time.unscaledTime;
            if (now - lastClickTime <= DoubleClickThreshold)
            {
                appManager.RequestOpenApp(def); 
            }
            lastClickTime = now;
        }
    }
}