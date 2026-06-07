using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    /// <summary>
    /// Yüklü uygulamaların ikonlarını masaüstünde gösterir.
    /// Çift tıklayınca DesktopController.OpenApp() çağrılır.
    ///
    /// SAHNE KURULUMU:
    ///   iconParent: Grid Layout Group olan RectTransform
    ///   iconPrefab: Image (ikon) + TMP (ad) + Button olan prefab
    /// </summary>
    public class DesktopIconGrid : MonoBehaviour
    {
        [SerializeField] private Computer computer;
        [SerializeField] private AppManager appManager;
        [SerializeField] private RectTransform iconParent;
        [SerializeField] private DesktopIcon iconPrefab;
        
        private void OnEnable()
        {
            computer.OnBootComplete += Refresh;
            appManager.OnAppsRefreshed += Refresh;
        }

        private void OnDisable()
        {
            if (computer != null) computer.OnBootComplete -= Refresh;
            if (appManager != null) appManager.OnAppsRefreshed -= Refresh;
        }

        private void Refresh()
        {
            Clear();
            foreach (var app in appManager.InstalledApps)
            {
                var icon = Instantiate(iconPrefab, iconParent);
                icon.Setup(app, appManager);
            }
        }

        private void Clear()
        {
            foreach (Transform child in iconParent)
                Destroy(child.gameObject);
        }
    }
}