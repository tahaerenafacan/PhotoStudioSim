using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class ItemPaperPrinter : MonoBehaviour, IInteractable, INetworkDevice
{
    [SerializeField] private NetworkDeviceSO networkData;
    [SerializeField] private LocalizedString interactHint;

    [SerializeField] private Material displayMat;
    
    [Header("Printer Settings")]
    [SerializeField] private PrintQuality maxSupportedQuality = PrintQuality.Average;
    [SerializeField] private bool isColoredSupported = true;
    [SerializeField] private int blackPagePerMinute = 10;
    [SerializeField] private int colorPagePerMinute = 5;
    
    [Header("Print Settings")]
    [SerializeField] private Transform paperSpawnPoint;    // Kağıdın çıkmaya başlayacağı yer
    [SerializeField] private Transform paperSpawnEndPoint; // Kağıdın çıktıktan sonra gittiği son nokta
    [SerializeField] private PrintedPaper prefabA3;
    [SerializeField] private PrintedPaper prefabA4;
    [SerializeField] private PrintedPaper prefabA5;
    
    public LocalizedString InteractHint => interactHint;
    public bool CanInteract => !isPrinting;
    public NetworkDeviceSO GetNetworkDeviceData() => networkData;
    public PrintQuality GetMaxSupportedQuality() => maxSupportedQuality;
    public bool GetIsColoredSupported() => isColoredSupported;
    
    private bool isPowered = false;
    private bool isPrinting = false;

    public void Interact()
    {
        if (isPrinting) return;
        
        isPowered = !isPowered;
        if (isPowered)
        {
            Router.Instance.Connect(this);
            displayMat.EnableKeyword("_EMISSION");
        }
        else
        {
            Router.Instance.Disconnect(this);
            displayMat.DisableKeyword("_EMISSION");
        }
    }
    
    // Galeriden çağırılacak fiziksel yazdırma metodu
    public void PrintDocument(PrintSettings settings, Texture2D imageToPrint)
    {
        if (!isPowered || isPrinting) return;

        PrintedPaper paperPrefabToSpawn = null;

        switch (settings.paperSize)
        {
            case PrintPaperSize.A3: paperPrefabToSpawn = prefabA3; break;
            case PrintPaperSize.A4: paperPrefabToSpawn = prefabA4; break;
            case PrintPaperSize.A5: paperPrefabToSpawn = prefabA5; break;
        }

        if (paperPrefabToSpawn != null && paperSpawnPoint != null)
        {
            PrintedPaper spawnedPaper = Instantiate(paperPrefabToSpawn, paperSpawnPoint.position, paperSpawnPoint.rotation);
            
            spawnedPaper.Setup(imageToPrint, settings);
            spawnedPaper.DisablePhysics();
            
            float ppm = settings.isColored ? colorPagePerMinute : blackPagePerMinute;
            float printDuration = 60f / ppm;

            StartCoroutine(AnimatePaperEject(spawnedPaper, printDuration));
        }
    }
    
    /// <summary>
    /// Kağıdı spawnPoint'ten endPoint'e fizik simüle etmeden kaydırır.
    /// Yan etki: isPrinting state'ini yönetir, dışarıdan erişilmemeli.
    /// </summary>
    private IEnumerator AnimatePaperEject(PrintedPaper paper, float duration)
    {
        isPrinting = true;

        Vector3 startPos = paperSpawnPoint.position;
        Vector3 endPos   = paperSpawnEndPoint.position;
        float elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            paper.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        paper.transform.position = endPos;
        paper.EnablePhysics();
        isPrinting = false;
    }
}
