using UnityEngine;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Simüle edilmiş internet'ten indirilebilecek bir öğenin konfigürasyonu.
    /// DownloadBrowserWindow bu SO'ları listeler; DownloadManager indirmeyi yönetir.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDownload", menuName = "PSSGame/Computer/Downloadable Item")]
    public class DownloadableItemSO : ScriptableObject
    {
        [Header("File Info")]
        public string fileName;
        public VirtualFileExtension fileExtension;
        public VirtualFileType fileType;
        public Sprite previewIcon;
        [TextArea] public string description;

        [Header("Download")]
        [Range(1f, 300f)] public float downloadTimeSeconds = 5f;
        public long fileSizeBytes = 1024 * 1024; // 1 MB

        [Header("Content (tipe göre biri kullanılır)")]
        public AppDefinition appToInstall;   // AppPackage
        public Texture2D     imageContent;   // Image
        [TextArea] public string textContent; // Document
    }
}