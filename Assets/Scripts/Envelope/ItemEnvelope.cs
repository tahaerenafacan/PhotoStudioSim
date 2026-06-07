using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StoredPhoto
{
    public Texture2D printedTexture;
    public PrintSettings settings;
}

public class ItemEnvelope : BasePickableItem
{
    [SerializeField] private int maxPhotoCount = 5;

    private List<StoredPhoto> storedPhotos = new List<StoredPhoto>();

    public IReadOnlyList<StoredPhoto> StoredPhotos => storedPhotos;
    public int CurrentPhotoCount => storedPhotos.Count;
    public bool HasSpace => storedPhotos.Count < maxPhotoCount;
    public bool IsFull => storedPhotos.Count >= maxPhotoCount;

    public bool TryStorePrintedPaper(PrintedPaper paper)
    {
        if (paper == null || paper.PrintedTexture == null || IsFull)
            return false;

        storedPhotos.Add(new StoredPhoto
        {
            printedTexture = paper.PrintedTexture,
            settings = paper.Settings
        });

        paper.gameObject.SetActive(false);
        return true;
    }
}
