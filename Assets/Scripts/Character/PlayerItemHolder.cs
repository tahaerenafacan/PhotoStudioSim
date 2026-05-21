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

    public event Action<bool, BasePickableItem> OnHeldItemChanged;

    [SerializeField] private ItemSway itemSway;

    [SerializeField] private Transform holdPoint;
    [SerializeField] private float dropForce = 2f;

    public bool IsHoldingItem => currentItem != null;
    public BasePickableItem CurrentItem => currentItem;
    public LocalizedString GetUseHint() => currentUsable.UseHint;

    private BasePickableItem currentItem;
    private IUsable currentUsable;
    private bool isUsingItem;
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

    private void Update()
    {
        HandleUseInput();
    }

    private void HandleUseInput()
    {
        if (!IsHoldingItem || currentUsable == null) return;

        if (InputManager.Instance.GetUseInputDown() && !isUsingItem)
        {
            isUsingItem = true;
            currentUsable.OnUseStart();
        }

        if (InputManager.Instance.GetUseInputUp() && isUsingItem)
        {
            isUsingItem = false;
            currentUsable.OnUseStop();
        }
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
        currentUsable = item as IUsable;

        item.OnPickup(holdPoint);

        OnHeldItemChanged?.Invoke(true, currentItem);

        return true;
    }

    private void Drop()
    {
        if (!IsHoldingItem) return;

        if (isUsingItem)
        {
            currentUsable?.OnUseStop();
            isUsingItem = false;
        }

        Vector3 dropDir = mainCamera ? mainCamera.transform.forward : transform.forward;

        currentItem.OnDrop(dropDir, dropForce);

        currentItem = null;
        currentUsable = null;
        OnHeldItemChanged?.Invoke(false, null);
    }

    public void ClearCurrentItem()
    {
        if (currentItem == null) return;

        if (isUsingItem)
        {
            currentUsable?.OnUseStop();
            isUsingItem = false;
        }

        if (currentItem is MonoBehaviour monoBehaviour)
        {
            Destroy(monoBehaviour.gameObject);
        }

        currentItem = null;
        currentUsable = null;
        OnHeldItemChanged?.Invoke(false, null);
    }
}