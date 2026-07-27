using System.Collections.Generic;
using System.Linq;
using SyntaxSultan.ComputerSystem.FileSystem;
using Computer.Apps.Gallery;
using SyntaxSultan.PrinterSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SyntaxSultan.ComputerSystem.Apps
{
    public class GalleryApp : AppWindow
    {
        [Header("Galeri UI")] 
        [SerializeField] private Transform thumbnailGrid;
        [SerializeField] private PhotoItem thumbnailPrefab;
        [SerializeField] private TextMeshProUGUI emptyLabel;
        [SerializeField] private TextMeshProUGUI currentPathText;

        [Header("Preview Panel")] 
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private Button printButton;

        [Header("Printing")] 
        [SerializeField] private PrintPopup printPopup;

        private VirtualFolder targetFolder;
        private List<Texture2D> photos = new();
        private List<VirtualFile> photoFiles = new();
        private int currentIndex = -1;

        protected override void Awake()
        {
            base.Awake();
            previewPanel.SetActive(false);
            printPopup.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            if (printPopup != null)
            {
                printPopup.OnPrintButtonClicked += StartPrinting;
            }
        }

        protected void OnDestroy()
        {
            if (printPopup != null)
            {
                printPopup.OnPrintButtonClicked -= StartPrinting;
            }
        }

        protected override void OnOpened()
        {
            previewPanel.SetActive(false);
            prevButton?.onClick.AddListener(ShowPrev);
            nextButton?.onClick.AddListener(ShowNext);
            printButton?.onClick.AddListener(PrintButtonClicked);

            // Context 3 türden biri olabilir:
            //  - VirtualFile  → dosya yöneticisinden bir fotoğrafa çift tıklandı: o dosyanın klasörü açılır ve o fotoğraf önizlemede seçili gelir
            //  - VirtualFolder → belirli bir klasör bağlamıyla açılış isteği (örn. USB izole görünüm)
            //  - null         → masaüstü ikonu: varsayılan iç disk Photos klasörü
            VirtualFile fileToPreview = Context as VirtualFile;
            targetFolder = fileToPreview?.Parent
                           ?? Context as VirtualFolder
                           ?? VirtualFileSystem.Instance.GetPhotosFolder();

            VirtualFileSystem.Instance.OnFileSystemChanged += HandleFolderChanged;
            RefreshGallery();

            if (fileToPreview != null)
            {
                int index = photoFiles.IndexOf(fileToPreview);
                if (index >= 0) ShowPhoto(index);
            }
        }

        protected override void OnClosed()
        {
            if (VirtualFileSystem.Instance != null)
                VirtualFileSystem.Instance.OnFileSystemChanged -= HandleFolderChanged;
        }

        private void HandleFolderChanged(VirtualFolder changedFolder)
        {
            if (changedFolder == targetFolder) RefreshGallery();
        }
        
        private void RefreshGallery()
        {
            photos.Clear();
            photoFiles.Clear();

            if (currentPathText) currentPathText.text = targetFolder != null ? targetFolder.GetFullPath() : "-";

            if (targetFolder != null)
            {
                foreach (var file in targetFolder.GetFiles())
                {
                    if (file.FileType != VirtualFileType.Image) continue;
                    var tex = file.GetContent<Texture2D>();
                    if (tex == null) continue;

                    photos.Add(tex);
                    photoFiles.Add(file); // Aynı index'te photos ile eşleşir; unique dosya adı sayesinde ShowPhoto ile geri bulunabilir
                }
            }

            FunctionLibrary.DestroyChildren(thumbnailGrid);

            bool hasPhotos = photos.Count > 0;
            if (emptyLabel) emptyLabel.gameObject.SetActive(!hasPhotos);

            if (!hasPhotos)
            {
                currentIndex = -1;
                UpdateNavBar();
                return;
            }

            // Thumbnail'ları oluştur
            for (int i = 0; i < photos.Count; i++)
            {
                int index = i;
                PhotoItem photo = Instantiate(thumbnailPrefab, thumbnailGrid);
                photo.SetImage(photos[index], () => ShowPhoto(index));
            }
        }

        private void ShowPhoto(int index)
        {
            if (photos.Count == 0) return;
            currentIndex = Mathf.Clamp(index, 0, photos.Count - 1);

            previewPanel.SetActive(true);
            if (previewImage) previewImage.texture = photos[currentIndex];
            UpdateNavBar();
        }

        private void ShowPrev() => ShowPhoto(currentIndex - 1);
        private void ShowNext() => ShowPhoto(currentIndex + 1);

        private void PrintButtonClicked()
        {
            printPopup.gameObject.SetActive(true);
            printPopup.SetPreviewImage(photos[currentIndex]);
        }

        private void StartPrinting(PrintSettings settings)
        {
            if (settings.targetPrinter == null)
            {
                Debug.LogError("Yazıcı seçilmedi veya ağda yazıcı yok!");
                return;
            }

            if (Router.Instance.GetNetworkDeviceComponent<ItemPaperPrinter>(settings.targetPrinter) is { } printer)
            {
                printer.PrintDocument(settings, photos[currentIndex]);
            }

            printPopup.gameObject.SetActive(false);
        }

        private void UpdateNavBar()
        {
            bool has = photos.Count > 0;
            if (prevButton) prevButton.interactable = has && currentIndex > 0;
            if (nextButton) nextButton.interactable = has && currentIndex < photos.Count - 1;
            if (indexText)
                indexText.text = has ? $"{currentIndex + 1} / {photos.Count}" : "0 / 0";
        }
    }
}