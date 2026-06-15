using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SyntaxSultan.ComputerSystem.FileSystem;

namespace SyntaxSultan.ComputerSystem.Apps
{
    /// <summary>
    /// Dosya Yöneticisi uygulaması — AppWindow'dan türer.
    ///
    /// PANEL DÜZENİ (prefab'da kurulması gereken):
    ///   Sol panel  → driveTreeParent (ScrollRect içinde VerticalLayoutGroup)
    ///   Sağ panel  → fileGridParent  (ScrollRect içinde GridLayoutGroup)
    ///   Alt toolbar → newFolderButton, deleteButton, copyToInternalButton, newFolderNameInput
    ///
    /// BAĞIMLILIK: Sahnede VirtualFileSystem singleton olmalı.
    /// </summary>
    public class FileManagerWindow : AppWindow
    {
        [Header("Config")]
        [SerializeField] private FolderIconConfig iconConfig;
        
        [Header("Panels")]
        [SerializeField] private Transform driveTreeParent;
        [SerializeField] private Transform fileGridParent;

        [SerializeField] private FileSystemEntryUI driveEntryPrefab;
        [SerializeField] private FileSystemEntryUI folderEntryPrefab;      // Sol tree
        [SerializeField] private FileSystemEntryUI folderGridEntryPrefab;  // Sağ grid ← YENİ
        [SerializeField] private FileSystemEntryUI fileEntryPrefab;

        [Header("Toolbar")]
        [SerializeField] private Evo.UI.Button          newFolderButton;
        [SerializeField] private Evo.UI.Button          deleteButton;
        [SerializeField] private TMP_InputField  newFolderNameInput;
        [SerializeField] private TextMeshProUGUI currentPathText;
        [SerializeField] private TextMeshProUGUI statusText;

        private VirtualFileSystem vfs;
        private VirtualFolder     currentFolder;
        private VirtualFSNode     selectedNode;
        private FileSystemEntryUI selectedEntry;

        protected override void OnOpened()
        {
            vfs = VirtualFileSystem.Instance;
            if (vfs == null) { Debug.LogError("[FileManager] VirtualFileSystem bulunamadı!"); return; }
        
            vfs.OnFileSystemChanged += HandleFileSystemChanged;
            vfs.OnDriveMounted      += _ => RefreshDriveTree();
            vfs.OnDriveUnmounted    += _ => RefreshDriveTree();
        
            newFolderButton.onClick.AddListener(CreateNewFolder);
            deleteButton.onClick.AddListener(DeleteSelected);
        
            RefreshDriveTree();
            NavigateTo(vfs.InternalRoot);

            LayoutRebuilder.ForceRebuildLayoutImmediate(driveTreeParent  as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(fileGridParent as RectTransform);
        }
        
        protected override void OnClosed()
        {
            if (vfs == null) return;
            vfs.OnFileSystemChanged -= HandleFileSystemChanged;
        }

        private void HandleFileSystemChanged(VirtualFolder changedFolder)
        {
            if (changedFolder == currentFolder) RefreshFileGrid();
            RefreshDriveTree();
        }

        private void NavigateTo(VirtualFolder folder)
        {
            currentFolder = folder;
            selectedNode  = null;
            selectedEntry = null;
            if (currentPathText) currentPathText.text = folder.GetFullPath();
            RefreshFileGrid();
        }

        // ── Sol Panel: Drive / Klasör Ağacı ─────────────────────────

        private void RefreshDriveTree()
        {
            ClearChildren(driveTreeParent);
            foreach (var drive in vfs.MountedDrives)
            {
                SpawnDriveEntry(drive);
                SpawnFolderTree(drive.Root, depth: 1);
            }
        }

        private void SpawnDriveEntry(IVirtualDrive drive)
        {
            var entry = Instantiate(driveEntryPrefab, driveTreeParent);
            var capturedRoot = drive.Root;
            entry.Setup($"[{drive.DriveName}] {drive.DriveLabel}",
                iconConfig?.driveIcon,
                () => NavigateTo(capturedRoot));
        }

        private void SpawnFolderTree(VirtualFolder folder, int depth)
        {
            foreach (var sub in folder.GetSubFolders())
            {
                var entry    = Instantiate(folderEntryPrefab, driveTreeParent);
                var captured = sub;
                Sprite icon  = iconConfig?.GetFolderIcon(sub.Name);
                entry.Setup(sub.Name, icon,
                    () => NavigateTo(captured), depth,
                    () => Select(captured, entry));
                //SpawnFolderTree(sub, depth + 1);
            }
        }

        // ── Sağ Panel: İçerik Grid ───────────────────────────────────

        private void RefreshFileGrid()
        {
            ClearChildren(fileGridParent);
            if (currentFolder == null) return;

            var gridFolderPrefab = folderGridEntryPrefab != null ? folderGridEntryPrefab : folderEntryPrefab;

            foreach (var sub in currentFolder.GetSubFolders())
            {
                var entry    = Instantiate(gridFolderPrefab, fileGridParent);
                var captured = sub;
                Sprite icon  = iconConfig?.GetFolderIcon(sub.Name);
                entry.Setup(sub.Name, icon,
                    () => NavigateTo(captured), 0,
                    () => Select(captured, entry));
            }

            foreach (var file in currentFolder.GetFiles())
            {
                var entry    = Instantiate(fileEntryPrefab, fileGridParent);
                var captured = file;
                entry.Setup($"{file.Name}.{file.Extension}",
                    iconConfig?.GetFileIconByType(file.FileType),              // ← ileride fileType'a göre genişletilebilir
                    null, 0,
                    () => Select(captured, entry));
            }

            UpdateStatus();
        }

        // ── Toolbar Actions ──────────────────────────────────────────

        private void CreateNewFolder()
        {
            if (currentFolder == null) return;
            string newFolderName = string.IsNullOrWhiteSpace(newFolderNameInput.text) ? "New Folder" : newFolderNameInput.text.Trim();
            vfs.CreateFolder(currentFolder, newFolderName);
            newFolderNameInput.text = string.Empty;
        }

        private void DeleteSelected()
        {
            if (selectedNode == null) return;
            if      (selectedNode is VirtualFile   f) vfs.DeleteFile(f);
            else if (selectedNode is VirtualFolder d) vfs.DeleteFolder(d);
            selectedNode  = null;
            selectedEntry = null;
        }

        /// <summary>
        /// Seçili dosyayı (genellikle USB'deki) iç sürücünün Documents klasörüne kopyalar.
        /// </summary>
        private void CopySelectedToInternal()
        {
            if (selectedNode is not VirtualFile file) return;
            var target = vfs.InternalRoot.FindChild("Documents") as VirtualFolder
                         ?? vfs.InternalRoot;
            bool ok = vfs.CopyFile(file, target);
            if (statusText) statusText.text = ok ? "Dosya kopyalandı." : "Kopyalama başarısız.";
        }

        // ── Helpers ──────────────────────────────────────────────────

        private void Select(VirtualFSNode node, FileSystemEntryUI entry)
        {
            selectedEntry?.SetSelected(false);
            selectedNode  = node;
            selectedEntry = entry;
            entry.SetSelected(true);
        }

        private void UpdateStatus()
        {
            if (statusText == null || currentFolder == null) return;
            int folders = currentFolder.GetSubFolders().Count();
            int files   = currentFolder.GetFiles().Count();
            statusText.text = $"{folders} klasör · {files} dosya";
        }

        private static void ClearChildren(Transform parent)
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }
    }
}