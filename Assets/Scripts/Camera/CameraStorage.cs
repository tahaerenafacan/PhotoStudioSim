using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Bilgisayara aktarılmış fotoğrafların merkezi deposu.
/// GameCamera buraya upload eder, GalleryApp buradan okur.
/// </summary>
public class CameraStorage : MonoBehaviour
{
    public static CameraStorage Instance { get; private set; }

    private readonly List<Texture2D> photos = new();
    private string savedPhotosPath;
    
    public IReadOnlyList<Texture2D> Photos => photos;
    public event System.Action OnPhotosChanged;
    public int Count => photos.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        savedPhotosPath = Path.Combine(Application.persistentDataPath, "SavedPhotos");
        LoadPhotosFromFolder();
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

    private void LoadPhotosFromFolder()
    {
        if (!Directory.Exists(savedPhotosPath)) return;
        
        foreach (var file in Directory.EnumerateFiles(savedPhotosPath))
        {
            var loadedImage = LoadImageFromBytes(File.ReadAllBytes(file));
            if (loadedImage != null)
            {
                photos.Add(loadedImage);
            }
        }
    }
    
    private Texture2D LoadImageFromBytes(byte[] imageBytes)
    {
        // 1. Create a temporary placeholder texture (dimensions will be overwritten)
        Texture2D texture = new Texture2D(2, 2);
        
        // 2. Load the PNG/JPG byte array into the texture
        // This method automatically resizes the texture and determines the format
        bool isLoaded = texture.LoadImage(imageBytes); 
        
        if (!isLoaded)
        {
            Debug.LogError("Failed to load image bytes into texture.");
            return null;
        }

        return texture;
    }
}