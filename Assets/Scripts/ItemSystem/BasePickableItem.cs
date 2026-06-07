using UnityEngine;

/// <summary>
/// Yerden alınabilen tüm itemlerin temel sınıfı.
/// 
/// KULLANIM:
///   - Sadece alınabilen item  → BasePickableItem'ı extend et
///   - Alınabilen + kullanılabilen → BasePickableItem + IUsable implement et
///   - Alınabilen + interact     → BasePickableItem + IInteractable implement et
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class BasePickableItem : MonoBehaviour, IPickable
{
    [SerializeField] private ItemDefinition definition;
    [SerializeField] private float throwForceMultiplier = 1f;

    public string GetItemName() => definition.itemName.GetLocalizedString();
    public bool IsUseable => definition.isUseable;
    public bool IsHeld { get; private set; }
    
    protected ItemDefinition Definition => definition;
    protected Rigidbody Rb { get; private set; }
    private Collider[] colliders;

    
    /// <summary>ItemDefinition'ı runtime'da set etmek için (spawn edilen itemlar).</summary>
    public void Initialize(ItemDefinition def)
    {
        definition = def;
    }

    protected virtual void Awake()
    {
        if (!definition)
        {
            Debug.LogError($"[BasePickableItem] {gameObject.name} için ItemDefinition atanmamış!");
            return;
        }
        Rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    protected void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void OnPickup(Transform holdPoint)
    {
        IsHeld = true;

        // Disable Physics
        Rb.linearVelocity = Vector3.zero;
        Rb.angularVelocity = Vector3.zero;
        Rb.isKinematic = true;

        SetCollidersActive(false);

        // Join holdpoint
        transform.SetParent(holdPoint);
        SetLocalPosition(definition.holdPositionOffset);
        transform.localRotation = Quaternion.Euler(definition.holdRotationOffset);

        OnPickedUp();
    }

    public void OnDrop(Vector3 throwDirection, float throwForce)
    {
        IsHeld = false;

        // Remove from holdpoint
        transform.SetParent(null);

        // Enable Physics
        Rb.isKinematic = false;

        SetCollidersActive(true);
            
        // Apply throw force
        if (throwForce > 0f)
            Rb.AddForce(throwDirection * throwForce * throwForceMultiplier, ForceMode.Impulse);

        OnDropped(); 
    }

    protected void SetCollidersActive(bool active)
    {
        foreach (var col in colliders)
            col.enabled = active;
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