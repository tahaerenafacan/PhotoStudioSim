using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    public class AppManager : MonoBehaviour
    {
        [SerializeField] private List<AppDefinition> initialApps = new();

        private readonly List<AppDefinition> installedApps = new();
    
        public event Action<AppDefinition> OnAppOpened;
        public event Action<AppDefinition> OnAppClosed;
        public event Action OnAppsRefreshed;

        public IReadOnlyList<AppDefinition> InstalledApps => installedApps;

        private void Start()
        {
            foreach (var app in initialApps)
            {
                InstallApp(app);
            }
        }

        public void InstallApp(AppDefinition def)
        {
            if (!installedApps.Contains(def))
            {
                installedApps.Add(def);
                OnAppsRefreshed?.Invoke();
            }
        }

        public void UninstallApp(AppDefinition def)
        {
            if (installedApps.Contains(def))
            {
                RequestCloseApp(def);
                installedApps.Remove(def);
                OnAppsRefreshed?.Invoke();
            }
        }

        public void RequestOpenApp(AppDefinition def)
        {
            if (!installedApps.Contains(def)) return;
            OnAppOpened?.Invoke(def);
        }

        public void RequestCloseApp(AppDefinition def)
        {
            OnAppClosed?.Invoke(def);
        }
    }
}