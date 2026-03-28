using System;
using UnityEngine;

/// <summary>
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
/// </summary>
public class PlayerItemHolder : MonoBehaviour
{
    public static PlayerItemHolder Instance { get; private set; }

    [SerializeField] private ItemSway itemSway;

    [Header("Hold Point")]
    [Tooltip("Item'ın elde tutulacağı boş Transform (kamera çocuğu).")]
    [SerializeField] private Transform holdPoint;

    [Header("Bırakma / Fırlatma")]
    [Tooltip("G tuşuna basınca uygulanan ileri kuvvet.")]
    [SerializeField] private float dropForce = 2f;

    public bool IsHoldingItem => currentItem != null;

    public IPickable CurrentItem => currentItem;

    private IPickable currentItem;
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
    }

    private void Update()
    {
        HandleDropInput();
        HandleUseInput();
    }

    private void HandleDropInput()
    {
        if (!IsHoldingItem) return;

        if (InputManager.Instance.DropKeyPressed())
            Drop();
    }

    private void HandleUseInput()
    {
        if (!IsHoldingItem || currentUsable == null) return;

        // Sol tık basıldı → kullanımı başlat
        if (InputManager.Instance.GetUseInputDown() && !isUsingItem)
        {
            isUsingItem = true;
            currentUsable.OnUseStart();
        }

        // Sol tık bırakıldı → kullanımı durdur
        if (InputManager.Instance.GetUseInputUp() && isUsingItem)
        {
            isUsingItem = false;
            currentUsable.OnUseStop();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────

    public bool TryPickup(IPickable item)
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
    }

    public string GetUseHint() => currentUsable?.UseHint ?? string.Empty;
}