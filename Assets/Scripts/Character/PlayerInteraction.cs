using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// Oyuncunun önüne raycast atar, IPickable ve IInteractable tespiti yapar.
/// E tuşuna basınca uygun aksiyonu tetikler.
/// 
/// TEŞHİS ÖNCELIK SIRASI (aynı objede ikisi varsa):
///   1) Elde item yoksa + IPickable varsa → item al
///   2) Elde item varsa + IInteractable varsa → interact et
///   3) Elde item yoksa + sadece IInteractable varsa → interact et
/// 
/// SAHNE KURULUMU:
///   - Bu script Player (veya Camera) objesine ekle
///   - interactableLayer: Raycast'in çarpacağı layer'ları seç
///     (alınabilir ve/veya etkileşimli tüm objeler bu layer'larda olmalı)
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("Raycast Ayarları")]
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private LayerMask interactableLayer;


    public IPickable DetectedPickable { get; private set; }
    public IInteractable DetectedInteractable { get; private set; }
    public bool HasDetection => DetectedPickable != null || DetectedInteractable != null;
    public bool IsInteractionEnabled => shouldCheckInteraction;

    private bool shouldCheckInteraction = true;
    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
        if (!mainCam)
            Debug.LogError("[PlayerInteraction] Ana kamera bulunamadı!");
    }

    private void Update()
    {
        if (!shouldCheckInteraction) return;
        PerformRaycast();
        HandleInteractInput();
    }

    public void DisableInteraction()
    {
        shouldCheckInteraction = false;
        DetectedPickable = null;
        DetectedInteractable = null;
    }

    public void EnableInteraction()
    {
        shouldCheckInteraction = true;
    }

    private void PerformRaycast()
    {
        DetectedPickable = null;
        DetectedInteractable = null;

        if (!mainCam) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer)) return;

        Collider hitCol = hit.collider;

        if (!PlayerItemHolder.Instance.IsHoldingItem)
        {
            hitCol.TryGetComponent(out IPickable detectedPick);
            DetectedPickable = PlayerItemHolder.Instance.IsHoldingItem ? null : detectedPick;
        }
        
        DetectedInteractable = hitCol.GetComponent<IInteractable>();
    }

    private void HandleInteractInput()
    {
        if (!InputManager.Instance.InteractKeyPressed()) return;

        if (DetectedPickable != null)
        {
            PlayerItemHolder.Instance.TryPickup(DetectedPickable);
            return;
        }

        if (DetectedInteractable != null)
        {
            DetectedInteractable.Interact();
        }
    }
}