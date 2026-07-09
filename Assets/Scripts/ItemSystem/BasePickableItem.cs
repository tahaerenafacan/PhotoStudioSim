using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public abstract class BasePickableItem : MonoBehaviour, IPickable, IPlaceable
{
    [FormerlySerializedAs("definition")] 
    [SerializeField] private ItemDefinition itemData;
    [SerializeField] private float throwForceMultiplier = 1f;
    [SerializeField] private bool allowVerticalPlacement = false;


    public UnityEngine.Localization.LocalizedString GetItemName() => itemData.itemName;
    public bool IsHeld { get; private set; }
    
    protected ItemDefinition ItemData => itemData;
    protected Rigidbody Rb { get; private set; }
    private Collider[] colliders;
    private Renderer[] cachedRenderers;

    // ─────────────────────────────────────────────────────────────
    // IPlaceable Implementasyonu

    public Transform PlacementTransform => transform;
    public Collider[] PlacementColliders => colliders;
    public Renderer[] PlacementRenderers => cachedRenderers;
    public bool AllowVerticalPlacement => allowVerticalPlacement;


    public void SetPlacementCollidersEnabled(bool isEnabled)
    {
        TogglePhysics(isEnabled);
    }

    public void OnPlacementConfirmed()
    {
        TogglePhysics(true);
    }

    public void OnPlacementCancelled() { }

    //------------------------------------------------------------------
    
    /// <summary>ItemDefinition'ı runtime'da set etmek için (spawn edilen itemlar).</summary>
    public void Initialize(ItemDefinition def)
    {
        itemData = def;
    }

    protected virtual void Awake()
    {
        if (!itemData)
        {
            Debug.LogError($"[BasePickableItem] {gameObject.name} için ItemDefinition atanmamış!");
            return;
        }
        Rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    protected void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void OnPickup(Transform holdPoint)
    {
        IsHeld = true;
        TogglePhysics(false);

        // Join holdpoint
        transform.SetParent(holdPoint);
        SetLocalPosition(itemData.holdPositionOffset);
        transform.localRotation = Quaternion.Euler(itemData.holdRotationOffset);

        OnPickedUp();
    }

    public void OnDrop(Vector3 throwDirection, float throwForce)
    {
        IsHeld = false;
        transform.SetParent(null);        // Remove from holdpoint
        TogglePhysics(true);

        if (throwForce > 0f)
        {
            Rb.AddForce(throwDirection * throwForce * throwForceMultiplier, ForceMode.Impulse);
        }

        OnDropped(); 
    }
    
    public void TogglePhysics(bool isEnabled)
    {
        if (!isEnabled)
        {
            Rb.linearVelocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
        }
        Rb.isKinematic = !isEnabled;

        foreach (var col in colliders)
        {
            if (col != null) col.enabled = isEnabled;
        }
    }

    /// <summary>Item eline alındıktan hemen sonra çağrılır.</summary>
    protected virtual void OnPickedUp() { }

    /// <summary>Item bırakıldıktan hemen sonra çağrılır.</summary>
    protected virtual void OnDropped() { }
    
    
    // ─────────────────────────────────────────────────────────────
    // Envanter Entegrasyonu

    /// <summary>
    /// Hold point'ten ayrılır; fizik ve collider durumu korunur.
    /// Yalnızca PlayerItemHolder.DetachForStorage() tarafından çağrılmalıdır.
    /// </summary>
    public void PrepareForStorage()
    {
        IsHeld = false;
        transform.SetParent(null);
        // Rb.isKinematic ve colliders kasıtlı olarak değiştirilmez;
        // envantere koyulana kadar bu durumda bekler.
    }

    /// <summary>
    /// Item'ı storage objesine taşır ve sahneden gizler (yok etmez).
    /// InventorySlot.TryStore() tarafından çağrılır.
    /// </summary>
    public void StoreInInventory(Transform storageRoot)
    {
        transform.SetParent(storageRoot);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Item'ı storage'dan geri getirir. Ardından TryPickup() beklenir.
    /// InventorySlot.Take() tarafından çağrılır.
    /// </summary>
    public void RetrieveFromInventory()
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
        // OnPickup() fizik ve collider'ları zaten düzenler.
    }
}