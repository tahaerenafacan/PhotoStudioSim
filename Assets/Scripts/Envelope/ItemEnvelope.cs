using System.Collections.Generic;
using SyntaxSultan.InventoryModule;
using SyntaxSultan.PrinterSystem;
using UnityEngine;

[System.Serializable]
public struct StoredPhoto
{
    public Texture2D printedTexture;
    public PrintSettings settings;
}

public class ItemEnvelope : BasePickableItem, IStorable
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

    public bool CanStore => true;
    public Sprite Icon => ItemData.icon;
}
