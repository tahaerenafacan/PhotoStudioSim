using UnityEngine;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// CameraStorage ile VirtualFileSystem arasındaki köprü (OCP — mevcut koda dokunmaz).
    /// CameraStorage.OnPhotosChanged tetiklenince yeni fotoğrafları VFS/Photos klasörüne yazar.
    ///
    /// SAHNE KURULUMU: Herhangi bir sahne objesine ekle (Computer objesi önerilir).
    /// </summary>
    public class CameraStorageToVFSBridge : MonoBehaviour
    {
        private int lastSyncedCount;

        private void Start()
        {
            if (CameraStorage.Instance != null)
                CameraStorage.Instance.OnPhotosChanged += SyncNewPhotos;
        }

        private void OnDestroy()
        {
            if (CameraStorage.Instance != null)
                CameraStorage.Instance.OnPhotosChanged -= SyncNewPhotos;
        }

        /// <summary>
        /// Sadece yeni eklenen (lastSyncedCount'tan sonraki) fotoğrafları sync eder.
        /// Tüm listeyi tekrar işlemez — performans optimizasyonu.
        /// </summary>
        private void SyncNewPhotos()
        {
            var storage = CameraStorage.Instance;
            var vfs     = VirtualFileSystem.Instance;
            if (storage == null || vfs == null) return;

            var photosFolder = vfs.GetPhotosFolder();
            if (photosFolder == null) return;

            for (int i = lastSyncedCount; i < storage.Count; i++)
            {
                var photo = storage.Photos[i];
                if (photo == null) continue;

                string name = $"Photo_{System.DateTime.Now:yyyyMMdd_HHmmss}_{i}";
                long approxSize = photo.width * photo.height * 3L;
                vfs.CreateFile(photosFolder, name, VirtualFileExtension.PNG, VirtualFileType.Image, photo, approxSize);
            }

            lastSyncedCount = storage.Count;
        }
    }
}