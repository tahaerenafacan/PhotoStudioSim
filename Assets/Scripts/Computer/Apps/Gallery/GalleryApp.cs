using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryApp : AppWindow
{
    [Header("Galeri UI")]
    [SerializeField] private Transform thumbnailGrid;
    [SerializeField] private GameObject thumbnailPrefab;

    [Header("Önizleme")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI indexText;

    [Header("Boş Durum")]
    [SerializeField] private TextMeshProUGUI emptyLabel;

    private List<Texture2D> photos = new();
    private int currentIndex = -1;

    protected override void Start()
    {
        previewPanel.SetActive(false);
    }

    protected override void OnOpened()
    {
        previewPanel.SetActive(false);
        prevButton?.onClick.AddListener(ShowPrev);
        nextButton?.onClick.AddListener(ShowNext);

        CameraStorage.Instance.OnPhotosChanged += RefreshGallery;
        RefreshGallery();
    }

    protected override void OnClosed()
    {
        CameraStorage.Instance.OnPhotosChanged -= RefreshGallery;
    }

    // ── Galeri ──────────────────────────────────────────────────

    private void RefreshGallery()
    {
        photos = new List<Texture2D>(CameraStorage.Instance.Photos);
        
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
            int idx = i;
            GameObject thumb = Instantiate(thumbnailPrefab, thumbnailGrid);
            thumb.GetComponentInChildren<RawImage>().texture = photos[i];

            // Tıklanınca önizlemeye geç
            Button btn = thumb.GetComponent<Button>();
            if (btn == null) btn = thumb.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowPhoto(idx));
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

    private void UpdateNavBar()
    {
        bool has = photos.Count > 0;
        if (prevButton) prevButton.interactable = has && currentIndex > 0;
        if (nextButton) nextButton.interactable = has && currentIndex < photos.Count - 1;
        if (indexText)
            indexText.text = has ? $"{currentIndex + 1} / {photos.Count}" : "0 / 0";
    }
}