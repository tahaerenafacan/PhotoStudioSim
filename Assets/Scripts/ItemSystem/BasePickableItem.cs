using UnityEngine;

/// <summary>
/// Yerden alınabilen tüm itemlerin temel sınıfı.
/// 
/// KULLANIM:
///   - Sadece alınabilen item  → BasePickableItem'ı extend et
///   - Alınabilen + kullanılabilen → BasePickableItem + IUsable implement et
///   - Alınabilen + interact     → BasePickableItem + IInteractable implement et
/// 
/// Rigidbody ve Collider yönetimi otomatik yapılır:
///   Elde tutulurken  → Rigidbody kinematic, Collider kapalı
///   Yere düşünce     → Rigidbody fiziksel, Collider açık
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class BasePickableItem : MonoBehaviour, IPickable
{
    [SerializeField] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    [Header("Fırlatma")]
    [SerializeField] private float throwForceMultiplier = 1f;

    public string ItemName => definition.itemName.GetLocalizedString();
    public string PickupHint => definition != null ? definition.itemName.GetLocalizedString() : "Al";
    
    protected Rigidbody Rb { get; private set; }
    private Collider[] colliders;

    public bool IsHeld { get; private set; }
    
    /// <summary>ItemDefinition'ı runtime'da set etmek için (spawn edilen itemlar).</summary>
    public void Initialize(ItemDefinition def) => definition = def;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    // ─────────────────────────────────────────────────────────────
    // IPickable Implementasyonu
    // ─────────────────────────────────────────────────────────────

    protected void SetLocalPosition(Vector3 position)
    {
        transform.localPosition =  position;
    }

    public virtual void OnPickup(Transform holdPoint)
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

    public virtual void OnDrop(Vector3 throwDirection, float throwForce)
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
}