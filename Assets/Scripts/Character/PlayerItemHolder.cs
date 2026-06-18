using System;
using UnityEngine;
using UnityEngine.Localization;

/// Oyuncunun elindeki itemi yönetir.
/// 
/// SORUMLULUKLAR:
///   - TryPickup()  → E tuşuna basınca PlayerInteraction çağırır
///   - Drop()       → G tuşuna basınca çağrılır
///   - Kullanım     → Sol tık basılı/bırakılınca IUsable'ı tetikler
/// 
/// SAHNE KURULUMU:
///   1) Bu script oyuncu kamerasının (veya Player objesinin) üzerine koy
///   2) HoldPoint: Kameranın biraz önünde ve aşağısında boş bir Transform oluştur,
///      buraya sürükle (örn. Camera/HoldPoint)

public class PlayerItemHolder : MonoBehaviour
{
    public static PlayerItemHolder Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private ItemSway itemSway;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float dropForce = 2f;

    public event Action<bool, BasePickableItem> OnHeldItemChanged;
    public event Action<IComplexUsable> OnInteractionOptionsChanged; 
    
    public bool IsHoldingItem => currentItem != null;
    public BasePickableItem CurrentItem => currentItem;

    private BasePickableItem currentItem;
    private IComplexUsable currentComplexUsable;
    private Camera mainCamera;

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
        mainCamera = Camera.main;
        InputManager.Instance.OnDropKeyPressed += Drop;
    }

    public void BindExternalInteraction(IComplexUsable complexUsable)
    {
        currentComplexUsable = complexUsable;
        BindInteractions();
    }

    public void UnbindExternalInteraction()
    {
        UnbindInteractions();
        currentComplexUsable = null;
    }
    
    private void BindInteractions()
    {
        if (currentComplexUsable == null || currentComplexUsable.GetInteractions() == null) return;

        foreach (var interaction in currentComplexUsable.GetInteractions())
        {
            if (interaction.ActionReference == null) continue;

            var action = interaction.ActionReference.action;
            action.Enable();

            if (interaction.OnStarted != null) action.started += interaction.OnStarted;
            if (interaction.OnPerformed != null) action.performed += interaction.OnPerformed;
            if (interaction.OnCanceled != null) action.canceled += interaction.OnCanceled;
        }
        
        OnInteractionOptionsChanged?.Invoke(currentComplexUsable);
    }

    private void UnbindInteractions()
    {
        if (currentComplexUsable == null || currentComplexUsable.GetInteractions() == null) return;

        foreach (var interaction in currentComplexUsable.GetInteractions())
        {
            if (interaction.ActionReference == null) continue;

            var action = interaction.ActionReference.action;

            if (interaction.OnStarted != null) action.started -= interaction.OnStarted;
            if (interaction.OnPerformed != null) action.performed -= interaction.OnPerformed;
            if (interaction.OnCanceled != null) action.canceled -= interaction.OnCanceled;

            action.Disable();
        }
        
        OnInteractionOptionsChanged?.Invoke(null);
    }
    

    // ─────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────

    public bool TryPickup(BasePickableItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[PlayerItemHolder] TryPickup: item null!");
            return false;
        }

        if (IsHoldingItem)
        {
            Debug.Log("[PlayerItemHolder] Elde zaten item var, alamazsın.");
            return false;
        }

        currentItem = item;
        currentComplexUsable = item as IComplexUsable;

        item.OnPickup(holdPoint);

        BindInteractions();
        
        OnHeldItemChanged?.Invoke(true, currentItem);

        return true;
    }
    
    /// <summary>
    /// Elde tutulan itemi envanter transferi için serbest bırakır.
    /// Drop()'tan farklı olarak fizik kuvveti uygulamaz, item sahneye bırakılmaz.
    ///
    /// ÇAĞIRAN: InventoryController (TryStoreHeldItem / SwapHeldWithSlot)
    /// </summary>
    public BasePickableItem DetachForStorage()
    {
        if (!IsHoldingItem) return null;

        UnbindInteractions();

        var detachedItem = currentItem;
        currentItem.PrepareForStorage();

        currentItem = null;
        currentComplexUsable = null;
        OnHeldItemChanged?.Invoke(false, null);
        return detachedItem;
    }

    private void Drop()
    {
        if (!IsHoldingItem) return;

        UnbindInteractions();

        Vector3 dropDir = mainCamera ? mainCamera.transform.forward : transform.forward;

        currentItem.OnDrop(dropDir, dropForce);

        currentItem = null;
        currentComplexUsable = null;
        OnHeldItemChanged?.Invoke(false, null);
    }

    public void ClearCurrentItem()
    {
        if (currentItem == null) return;

        UnbindInteractions();

        if (currentItem is MonoBehaviour monoBehaviour)
        {
            Destroy(monoBehaviour.gameObject);
        }

        currentItem = null;
        currentComplexUsable = null;
        OnHeldItemChanged?.Invoke(false, null);
    }
}