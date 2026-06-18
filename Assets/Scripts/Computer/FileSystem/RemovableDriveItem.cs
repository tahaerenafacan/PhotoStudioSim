using SyntaxSultan.InventoryModule;
using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Dünya sahnesinde pickable olan çıkarılabilir sürücü (USB, SD kart).
    /// Oyuncu bu itemi eline alıp DrivePort'a götürünce mount edilir.
    ///
    /// SAHNE KURULUMU:
    ///   1. USB prefab'ına ekle
    ///   2. preloadedFiles alanına önceden yüklü içerik atanabilir (isteğe bağlı)
    ///   3. DrivePort bileşeni mount/unmount işlemini yönetir
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RemovableDriveItem : BasePickableItem, IStorable
    {
        [Header("Drive Settings")]
        [SerializeField] private string driveName  = "USB";
        [SerializeField] private string driveLabel = "Removable Drive";
        [SerializeField] private long   capacityMB = 32;

        [Header("Pre-loaded Content")]
        [Tooltip("Sürücü ile birlikte gelen dosyalar (örn: görev verileri, fotoğraflar)")]
        [SerializeField] private DownloadableItemSO[] preloadedFiles;

        public bool   CanStore => true;
        public Sprite Icon     => ItemData.icon;

        private VirtualDrive drive;
        public  IVirtualDrive Drive => drive;

        protected override void Awake()
        {
            base.Awake();
            drive = new VirtualDrive(driveName, driveLabel, capacityMB * 1024 * 1024, isRemovable: true);
            PopulatePreloadedFiles();
        }

        /// <summary>
        /// Inspector'da atanan ScriptableObject dosyalarını sürücünün root klasörüne yazar.
        /// Oyun dünyasında zaten içerik yüklenmiş USB simülasyonu için kullanılır.
        /// </summary>
        private void PopulatePreloadedFiles()
        {
            if (preloadedFiles == null) return;
            foreach (var item in preloadedFiles)
            {
                if (item == null) continue;
                object content = item.fileType switch
                {
                    VirtualFileType.Image      => (object)item.imageContent,
                    VirtualFileType.Document   => item.textContent,
                    VirtualFileType.AppPackage => item.appToInstall,
                    _                          => null
                };
                drive.Root.AddChild(
                    new VirtualFile(item.fileName, item.fileExtension,
                        item.fileType, content, item.fileSizeBytes));
            }
        }

        protected override void OnPickedUp() { }
        protected override void OnDropped()  { }
    }
}