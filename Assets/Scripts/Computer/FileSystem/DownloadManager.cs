using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    public class DownloadTask
    {
        public DownloadableItemSO Item { get; }
        public float Progress { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsCancelled { get; private set; }

        public event Action<float> OnProgressChanged;
        public event Action<DownloadTask> OnCompleted;

        public DownloadTask(DownloadableItemSO item)
        {
            Item = item;
        }

        internal void SetProgress(float v)
        {
            Progress = Mathf.Clamp01(v);
            OnProgressChanged?.Invoke(Progress);
        }

        internal void Complete()
        {
            IsComplete = true;
            Progress = 1f;
            OnCompleted?.Invoke(this);
        }

        public void Cancel() => IsCancelled = true;
    }

    /// <summary>
    /// Simüle edilmiş internet indirme yöneticisi.
    ///
    /// KULLANIM: DownloadManager.Instance.StartDownload(itemSO)
    ///
    /// YAN ETKİ: AppPackage tipinde dosya indirilirse AppManager'a otomatik install edilir.
    ///           İndirilen dosya VirtualFileSystem.Downloads klasörüne eklenir.
    /// </summary>
    public class DownloadManager : MonoBehaviour
    {
        public static DownloadManager Instance { get; private set; }

        [SerializeField] private AppManager appManager;

        private readonly List<DownloadTask> activeDownloads = new();
        public IReadOnlyList<DownloadTask> ActiveDownloads => activeDownloads;

        public event Action<DownloadTask> OnDownloadStarted;
        public event Action<DownloadTask> OnDownloadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public DownloadTask StartDownload(DownloadableItemSO item)
        {
            // Aynı dosyayı çift indirme engeli
            if (activeDownloads.Exists(d => d.Item == item && !d.IsComplete))
            {
                Debug.LogWarning($"[DownloadMgr] '{item.fileName}' zaten indiriliyor.");
                return null;
            }
            var task = new DownloadTask(item);
            activeDownloads.Add(task);
            OnDownloadStarted?.Invoke(task);
            StartCoroutine(DownloadCoroutine(task));
            return task;
        }

        private IEnumerator DownloadCoroutine(DownloadTask task)
        {
            float elapsed = 0f;
            float duration = task.Item.downloadTimeSeconds;

            while (elapsed < duration)
            {
                if (task.IsCancelled) { activeDownloads.Remove(task); yield break; }
                elapsed += Time.deltaTime;
                task.SetProgress(elapsed / duration);
                yield return null;
            }

            task.Complete();
            activeDownloads.Remove(task);
            SaveToFileSystem(task.Item);
            OnDownloadCompleted?.Invoke(task);
        }

        private void SaveToFileSystem(DownloadableItemSO item)
        {
            var vfs = VirtualFileSystem.Instance;
            var downloadsFolder = vfs?.GetDownloadsFolder();
            if (downloadsFolder == null) return;

            object content = item.fileType switch
            {
                VirtualFileType.Image      => (object)item.imageContent,
                VirtualFileType.Document   => item.textContent,
                VirtualFileType.AppPackage => item.appToInstall,
                _                          => null
            };

            vfs.CreateFile(downloadsFolder, item.fileName, item.fileExtension,
                item.fileType, content, item.fileSizeBytes);

            // App paketi ise AppManager'a otomatik install et
            if (item.fileType == VirtualFileType.AppPackage && item.appToInstall != null)
                appManager?.InstallApp(item.appToInstall);
        }
    }
}