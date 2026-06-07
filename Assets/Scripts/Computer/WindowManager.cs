using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SyntaxSultan.ComputerSystem
{

    public class WindowManager : MonoBehaviour
    {
        [SerializeField] private Computer computer;
        [SerializeField] private AppManager appManager;
        
        [Header("UI Components")]
        [SerializeField] private ComputerSplashScreen splashScreen;
        [SerializeField] private RectTransform shutdownScreen;
        [SerializeField] private RectTransform windowParent;
        [SerializeField] private RectTransform monitorPanel; 

        [Header("Timing")]
        [SerializeField] private float computerOpenTime = 3f;
        [SerializeField] private float computerShutdownTime = 1.5f;

        private readonly List<AppWindow> openWindows = new();
        private readonly Dictionary<AppDefinition, AppWindow> activeWindows = new();
        private Coroutine bootCoroutine;
        private Coroutine shutdownCoroutine;

        private void OnEnable()
        {
            computer.OnBootStart += HandleBootStart;
            computer.OnShutdownStart += HandleShutdownStart;
            
            appManager.OnAppOpened += OpenWindow;
            appManager.OnAppClosed += CloseWindowByDefinition;
        }

        private void OnDisable()
        {
            if (computer != null)
            {
                computer.OnBootStart -= HandleBootStart;
                computer.OnShutdownStart -= HandleShutdownStart;
            }
            if (appManager != null)
            {
                appManager.OnAppOpened -= OpenWindow;
                appManager.OnAppClosed -= CloseWindowByDefinition;
            }
        }

        private void Start()
        {
            if (monitorPanel != null) monitorPanel.gameObject.SetActive(false);
            if (splashScreen != null) splashScreen.gameObject.SetActive(false);
            if (shutdownScreen != null) shutdownScreen.gameObject.SetActive(false);
            windowParent.gameObject.SetActive(false);
        }

        private void HandleBootStart()
        {
            if (bootCoroutine != null) StopCoroutine(bootCoroutine);
            bootCoroutine = StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            if (monitorPanel != null) monitorPanel.gameObject.SetActive(true);
            
            splashScreen.gameObject.SetActive(true);
            CanvasGroup splashGroup = splashScreen.GetComponent<CanvasGroup>();
            splashGroup.alpha = 1;
            
            yield return new WaitForSeconds(computerOpenTime);
            
            if (splashGroup != null)
            {
                yield return splashGroup.DOFade(0, 0.25f).WaitForCompletion();
            }
            
            splashScreen.gameObject.SetActive(false);
            windowParent.gameObject.SetActive(true);
            
            computer.CompleteBoot();
        }

        private void HandleShutdownStart()
        {
            if (shutdownCoroutine != null) StopCoroutine(shutdownCoroutine);
            shutdownCoroutine = StartCoroutine(ShutdownSequence());
        }

        private IEnumerator ShutdownSequence()
        {
            windowParent.gameObject.SetActive(false);
            
            foreach (var aw in openWindows.ToArray())
            {
                if (aw != null) Destroy(aw.gameObject);
            }
            openWindows.Clear();
            activeWindows.Clear();

            shutdownScreen.gameObject.SetActive(true);
            CanvasGroup shutdownGroup = shutdownScreen.GetComponent<CanvasGroup>();
            
            if (shutdownGroup != null)
            {
                shutdownGroup.alpha = 0;
                shutdownGroup.DOFade(1, 0.5f);
            }

            yield return new WaitForSeconds(computerShutdownTime);
            
            shutdownScreen.gameObject.SetActive(false);
            if (monitorPanel != null) monitorPanel.gameObject.SetActive(false);
            
            computer.CompleteShutdown();
        }

        public void OpenWindow(AppDefinition def)
        {
            if (def.windowPrefab == null) return;

            if (activeWindows.TryGetValue(def, out var existingWindow))
            {
                BringToFront(existingWindow);
                return;
            }

            AppWindow window = Instantiate(def.windowPrefab, windowParent);
            window.Setup(def, this);

            openWindows.Add(window);
            activeWindows[def] = window;
            BringToFront(window);
        }

        public void NotifyWindowClosed(AppWindow window)
        {
            if (!openWindows.Contains(window)) return;
            
            activeWindows.Remove(window.Definition);
            openWindows.Remove(window);
            Destroy(window.gameObject);
        }

        private void CloseWindowByDefinition(AppDefinition def)
        {
            if (activeWindows.TryGetValue(def, out var window))
            {
                NotifyWindowClosed(window);
            }
        }

        public void BringToFront(AppWindow window)
        {
            window.transform.SetAsLastSibling();
        }
    }
}