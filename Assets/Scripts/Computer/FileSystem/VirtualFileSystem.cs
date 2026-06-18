using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Oyun içi sanal dosya sisteminin merkezi yöneticisi (SSOT).
    ///
    /// SORUMLULUKLAR:
    ///   - İç sürücü ve takılabilir sürücülerin mount/unmount yönetimi
    ///   - Dosya/klasör CRUD
    ///   - Sürücüler arası dosya kopyalama (USB → iç disk)
    ///
    /// ÇAĞIRAN: FileManagerWindow, DownloadManager, DrivePort, CameraStorageBridge
    /// </summary>
    public class VirtualFileSystem : MonoBehaviour
    {
        public static VirtualFileSystem Instance { get; private set; }

        [Header("Internal Drive")]
        [SerializeField] private string internalDriveName  = "C";
        [SerializeField] private string internalDriveLabel = "System";
        [SerializeField] private long   internalCapacityMB = 512;

        private VirtualDrive internalDrive;
        private readonly List<IVirtualDrive> mountedDrives = new();

        public event Action<IVirtualDrive>  OnDriveMounted;
        public event Action<IVirtualDrive>  OnDriveUnmounted;
        public event Action<VirtualFolder>  OnFileSystemChanged;

        public IReadOnlyList<IVirtualDrive> MountedDrives => mountedDrives;
        public VirtualFolder InternalRoot => internalDrive.Root;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeInternalDrive();
        }

        private void InitializeInternalDrive()
        {
            internalDrive = new VirtualDrive(
                internalDriveName, internalDriveLabel,
                internalCapacityMB * 1024 * 1024
            );
            // Varsayılan klasör yapısı
            CreateFolder(internalDrive.Root, "Documents");
            CreateFolder(internalDrive.Root, "Downloads");
            CreateFolder(internalDrive.Root, "Photos");
            CreateFolder(internalDrive.Root, "Applications");
            mountedDrives.Add(internalDrive);
        }

        // ── Drive API ────────────────────────────────────────────────

        public void MountDrive(IVirtualDrive drive)
        {
            if (mountedDrives.Contains(drive)) return;
            mountedDrives.Add(drive);
            OnDriveMounted?.Invoke(drive);
        }

        public void UnmountDrive(IVirtualDrive drive)
        {
            if (drive == internalDrive) return; // İç disk çıkarılamaz
            if (!mountedDrives.Remove(drive))   return;
            OnDriveUnmounted?.Invoke(drive);
        }

        // ── Folder API ───────────────────────────────────────────────

        public VirtualFolder CreateFolder(VirtualFolder parent, string name)
        {
            var folder = new VirtualFolder(name);
            if (!parent.AddChild(folder))
            {
                Debug.LogWarning($"[VFS] '{name}' zaten var: {parent.GetFullPath()}");
                return parent.FindChild(name) as VirtualFolder;
            }
            OnFileSystemChanged?.Invoke(parent);
            return folder;
        }

        public bool DeleteFolder(VirtualFolder folder)
        {
            if (folder.Parent == null) { Debug.LogWarning("[VFS] Root silinemez."); return false; }

            var parent = folder.Parent;
            parent.RemoveChild(folder);
            OnFileSystemChanged?.Invoke(parent);
            return true;
        }

        // ── File API ─────────────────────────────────────────────────

        public VirtualFile CreateFile(VirtualFolder parent, string fileName, VirtualFileExtension extension, VirtualFileType fileType, object content, long sizeBytes = 1024)
        {
            var file = new VirtualFile(fileName, extension, fileType, content, sizeBytes);
            if (!parent.AddChild(file))
            {
                Debug.LogWarning($"[VFS] '{fileName}' zaten var.");
                return null;
            }
            GetDriveForFolder(parent)?.AddUsedBytes(sizeBytes);
            OnFileSystemChanged?.Invoke(parent);
            return file;
        }

        public bool DeleteFile(VirtualFile file)
        {
            if (file.Parent == null) return false;
            var parent = file.Parent;
            GetDriveForFolder(parent)?.SubtractUsedBytes(file.SizeBytes);
            parent.RemoveChild(file);
            OnFileSystemChanged?.Invoke(parent);
            return true;
        }

        /// <summary>
        /// Dosyayı kaynak klasörden hedefe kopyalar.
        /// Tipik kullanım: USB sürücüden iç diske aktarım.
        /// Yan etki: Hedef sürücünün UsedBytes değeri artırılır.
        /// </summary>
        public bool CopyFile(VirtualFile source, VirtualFolder destination)
        {
            var drive = GetDriveForFolder(destination);
            if (drive is VirtualDrive vd && !vd.HasEnoughSpace(source.SizeBytes))
            {
                Debug.LogWarning("[VFS] Hedef sürücüde yeterli alan yok.");
                return false;
            }
            var copy = new VirtualFile(source.Name, source.Extension, source.FileType,
                source.GetContent<object>(), source.SizeBytes);
            if (!destination.AddChild(copy)) return false;
            (drive as VirtualDrive)?.AddUsedBytes(source.SizeBytes);
            OnFileSystemChanged?.Invoke(destination);
            return true;
        }

        // ── Convenience Accessors ─────────────────────────────────────

        public VirtualFolder GetDownloadsFolder() =>
            internalDrive.Root.FindChild("Downloads") as VirtualFolder;

        public VirtualFolder GetPhotosFolder() =>
            internalDrive.Root.FindChild("Photos") as VirtualFolder;

        // ── Private Helpers ──────────────────────────────────────────

        /// <summary>
        /// Klasörün ait olduğu sürücüyü root'a çıkarak bulur.
        /// Kullanım alanı notu: VirtualDrive olmayanlar (salt okunur) null döner.
        /// </summary>
        private VirtualDrive GetDriveForFolder(VirtualFolder folder)
        {
            VirtualFSNode current = folder;
            while (current.Parent != null) current = current.Parent;
            return mountedDrives.OfType<VirtualDrive>().FirstOrDefault(d => d.Root == current);
        }
    }
}