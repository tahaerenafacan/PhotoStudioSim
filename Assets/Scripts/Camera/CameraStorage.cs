using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bilgisayara aktarılmış fotoğrafların merkezi deposu.
/// GameCamera buraya upload eder, GalleryApp buradan okur.
/// </summary>
public class CameraStorage : MonoBehaviour
{
    public static CameraStorage Instance { get; private set; }

    private readonly List<Texture2D> photos = new();

    public IReadOnlyList<Texture2D> Photos => photos;

    public event System.Action OnPhotosChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Upload(IEnumerable<Texture2D> newPhotos)
    {
        bool changed = false;
        foreach (var t in newPhotos)
        {
            if (t != null && !photos.Contains(t))
            {
                photos.Add(t);
                changed = true;
            }
        }
        if (changed) OnPhotosChanged?.Invoke();
    }

    public int Count => photos.Count;
}