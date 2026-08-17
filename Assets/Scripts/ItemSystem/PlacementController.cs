using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Item placement sisteminin merkezi state machine'i (SSOT).
///
/// İKİ AYRI AKIŞ:
///   A) Elde BasePickableItem varken → 'V' TOGGLE olarak çalışır (aç/kapa),
///      sol tık ile geçerli konumdaysa yerleştirilir.
///   B) Elde item yokken, bakılan pickable-olmayan bir IPlaceable objesi varsa
///      → 'V' BASILI TUTULARAK taşınır, bırakılınca geçerliyse orada kalır,
///      değilse orijinal konumuna döner.
///
/// </summary>
public class PlacementController : MonoBehaviour
{
    public static PlacementController Instance { get; private set; }

    private enum PlacementMode { None, HeldItem, WorldObject }

    [Header("Input")]
    [SerializeField] private InputActionReference placeActionReference;

    [Header("Materials")]
    [SerializeField] private Material placeableMaterial;
    [SerializeField] private Material notPlaceableMaterial;

    [Header("Raycast Ayarları")]
    [SerializeField] private float maxPlacementDistance = 4f;
    [SerializeField] private float detectDistance = 4f;
    [SerializeField] private float surfaceOffset = 0.02f;
    [SerializeField] private LayerMask obstructionLayerMask = ~0;
    [SerializeField] private float maxSurfaceAngleForFlatPlacement = 45f;

    [Header("Player Colliders")]
    [SerializeField] private Collider[] playerIgnoreColliders;

    [Header("Rotasyon")]
    [SerializeField] private float rotationStepDegrees = 15f;

    public event System.Action<bool> OnPlacementValidityChanged;
    public bool IsInPlacementMode => currentMode != PlacementMode.None;

    private float manualYawOffset;
    private PlacementMode currentMode = PlacementMode.None;
    private IPlaceable activePlaceable;
    private bool isCurrentPoseValid;
    private readonly System.Collections.Generic.Dictionary<Renderer, Material[]> originalMaterials = new();
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (placeActionReference == null) return;
        placeActionReference.action.Enable();
        placeActionReference.action.performed += HandlePlacePerformed;
        placeActionReference.action.canceled += HandlePlaceCanceled;
    }

    private void OnDisable()
    {
        if (placeActionReference == null) return;
        placeActionReference.action.performed -= HandlePlacePerformed;
        placeActionReference.action.canceled -= HandlePlaceCanceled;
    }

    private void Update()
    {
        if (currentMode == PlacementMode.None) return;

        HandleRotationInput();
        UpdatePlacementPreview();
        HandleCancelInput();

        if (currentMode == PlacementMode.HeldItem)
        {
            HandleHeldItemConfirmInput();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Input Callbacks

    private void HandlePlacePerformed(InputAction.CallbackContext ctx)
    {
        // Case A: Elde item varken V = TOGGLE
        if (PlayerItemHolder.Instance.IsHoldingItem)
        {
            if (currentMode == PlacementMode.HeldItem) CancelHeldItemPlacement();
            else EnterHeldItemPlacement();
            return;
        }

        // Case B: Elde item yokken V = BASILI TUTMA ile world-object taşıma başlatır
        if (currentMode == PlacementMode.None && TryGetLookedAtPlaceable(out var placeable))
        {
            EnterWorldObjectPlacement(placeable);
        }
    }

    private void HandlePlaceCanceled(InputAction.CallbackContext ctx)
    {
        // Sadece hold-tabanlı (World Object) akış, V bırakılınca sonlanır.
        // Held-item akışı toggle olduğu için burada işlem yapılmaz.
        if (currentMode == PlacementMode.WorldObject)
        {
            FinishWorldObjectPlacement();
        }
    }

    /// <summary>
    /// Mouse scroll ile Y ekseninde 15°'lik adımlarla manuel rotasyon.
    /// X/Z ekseni kasıtlı olarak kontrol edilmez; obje placement sırasında
    /// her zaman dik durur (yüzey normaline hizalanmaz).
    /// </summary>
    private void HandleRotationInput()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll == 0f) return;

        manualYawOffset += (scroll > 0f ? rotationStepDegrees : -rotationStepDegrees);
        manualYawOffset %= 360f;
    }

    private void HandleCancelInput()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (currentMode == PlacementMode.HeldItem)
        {
            CancelHeldItemPlacement();
        }
        else if (currentMode == PlacementMode.WorldObject)
        {
            RevertWorldObjectToOriginalPose();
        }
    }

    private void HandleHeldItemConfirmInput()
    {
        if (!InputManager.Instance.GetUseInputDown()) return;
        if (!isCurrentPoseValid) return; // Geçersiz konumda place edilmez, mod açık kalır

        activePlaceable.OnPlacementConfirmed();
        activePlaceable.SetPlacementCollidersEnabled(true);
        ExitPlacementMode();
    }

    // ─────────────────────────────────────────────────────────────
    // Case A: Held Item Placement

    private void EnterHeldItemPlacement()
    {
        var item = PlayerItemHolder.Instance.DetachForStorage();
        if (item == null) return;

        manualYawOffset = 0f;
        activePlaceable = item;
        currentMode = PlacementMode.HeldItem;
        CacheOriginalMaterials(activePlaceable);
        PlayerInteraction.Instance.DisableInteraction();
    }

    private void CancelHeldItemPlacement()
    {
        if (activePlaceable is BasePickableItem item)
        {
            PlayerItemHolder.Instance.TryPickup(item);
        }
        ExitPlacementMode();
    }

    // ─────────────────────────────────────────────────────────────
    // Case B: World Object Placement (hold-to-drag)

    private void EnterWorldObjectPlacement(IPlaceable placeable)
    {
        activePlaceable = placeable;
        originalPosition = placeable.PlacementTransform.position;
        originalRotation = placeable.PlacementTransform.rotation;

        manualYawOffset = 0f;
        placeable.SetPlacementCollidersEnabled(false);
        currentMode = PlacementMode.WorldObject;
        CacheOriginalMaterials(placeable);
        PlayerInteraction.Instance.DisableInteraction();
    }

    private void FinishWorldObjectPlacement()
    {
        if (isCurrentPoseValid)
        {
            activePlaceable.OnPlacementConfirmed();
        }
        else
        {
            activePlaceable.PlacementTransform.SetPositionAndRotation(originalPosition, originalRotation);
            activePlaceable.OnPlacementCancelled();
        }

        activePlaceable.SetPlacementCollidersEnabled(true);
        ExitPlacementMode();
    }

    private void RevertWorldObjectToOriginalPose()
    {
        activePlaceable.PlacementTransform.SetPositionAndRotation(originalPosition, originalRotation);
        activePlaceable.SetPlacementCollidersEnabled(true);
        activePlaceable.OnPlacementCancelled();
        ExitPlacementMode();
    }

    private void ExitPlacementMode()
    {
        RestoreOriginalMaterials();
        activePlaceable = null;
        currentMode = PlacementMode.None;
        PlayerInteraction.Instance.EnableInteraction();
    }

    // ─────────────────────────────────────────────────────────────
    // Preview / Raycast

    private void UpdatePlacementPreview()
    {
        if (!TryCalculatePlacementPose(out Vector3 pose, out Quaternion rotation, out Vector3 surfaceNormal)) return;

        // Önce ham pozisyon/rotasyonu uygula; bounds hesaplaması güncel transform'a ihtiyaç duyar.
        activePlaceable.PlacementTransform.SetPositionAndRotation(pose, rotation);

        // Objenin altını (collider bounds'a göre) tam yüzeye oturt; içine gömülmesini engeller.
        Vector3 restingPose = ResolveRestingPosition(activePlaceable, pose, surfaceNormal);
        activePlaceable.PlacementTransform.position = restingPose;

        bool isFlatEnough = IsSurfaceAngleValid(surfaceNormal, activePlaceable.PlacementAllowance);
        bool wasValid = isCurrentPoseValid;
        isCurrentPoseValid = isFlatEnough && IsPoseValid(activePlaceable);

        if (wasValid != isCurrentPoseValid)
        {
            OnPlacementValidityChanged?.Invoke(isCurrentPoseValid);
            ApplyPreviewMaterial(isCurrentPoseValid);
        }
    }

    /// <summary>
    /// Nesnenin collider bounds'una göre, yüzey normali doğrultusunda en alt noktasının
    /// tam olarak vuruş noktasında olacağı pozisyonu hesaplar. Bu olmadan pivot ortadaysa
    /// nesne yüzeyin içine gömülür (örn. masa üstüne konurken masanın içine batması).
    /// </summary>
    private Vector3 ResolveRestingPosition(IPlaceable placeable, Vector3 rawPosition, Vector3 normal)
    {
        Bounds? combinedBounds = null;
        foreach (var col in placeable.PlacementColliders)
        {
            if (col == null) continue;
            if (combinedBounds == null) combinedBounds = col.bounds;
            else
            {
                var b = combinedBounds.Value;
                b.Encapsulate(col.bounds);
                combinedBounds = b;
            }
        }

        if (combinedBounds == null) return rawPosition;

        // AABB'nin normal yönündeki en alt (min) desteğini bul (support function).
        Bounds bounds = combinedBounds.Value;
        float supportRadius = Mathf.Abs(bounds.extents.x * normal.x)
                             + Mathf.Abs(bounds.extents.y * normal.y)
                             + Mathf.Abs(bounds.extents.z * normal.z);
        float centerProjection = Vector3.Dot(bounds.center, normal);
        float minProjection = centerProjection - supportRadius;

        float targetProjection = Vector3.Dot(rawPosition, normal) + surfaceOffset;
        float correction = targetProjection - minProjection;

        return rawPosition + normal * correction;
    }

    /// <summary>Yüzey normali yeterince "yatay/yere yakın" değilse (duvar vb.) ve item izin vermiyorsa geçersizdir.</summary>
    private bool IsSurfaceAngleValid(Vector3 surfaceNormal, IPlaceable.PlacementType placementType)
    {
        float angleFromUp = Vector3.Angle(surfaceNormal, Vector3.up);

        return placementType switch
        {
            IPlaceable.PlacementType.OnlyHorizontal => angleFromUp <= maxSurfaceAngleForFlatPlacement,
            IPlaceable.PlacementType.OnlyVertical => angleFromUp >= (90f - maxSurfaceAngleForFlatPlacement),
            IPlaceable.PlacementType.Both => true,
            _ => true
        };    
    }

    private bool TryCalculatePlacementPose(out Vector3 position, out Quaternion rotation, out Vector3 surfaceNormal)
    {
        position = default;
        rotation = default;
        surfaceNormal = Vector3.up;
        if (mainCamera == null) return false;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, obstructionLayerMask, QueryTriggerInteraction.Ignore))
        {
            position = hit.point + hit.normal * surfaceOffset;
            surfaceNormal = hit.normal;
        }
        else
        {
            position = ray.origin + ray.direction * maxPlacementDistance;
        }

        // Obje her zaman dik durur; X/Z rotasyonu kasıtlı olarak sıfır.
        // Y rotasyonu: kameranın baktığı yön + manuel scroll offseti.
        float yaw = mainCamera.transform.eulerAngles.y + manualYawOffset;
        rotation = Quaternion.Euler(0f, yaw, 0f);

        return true;
    }

    /// <summary>
    /// Nesnenin collider'ları placement sırasında disable edildiği için
    /// Physics.OverlapBox onları zaten sonuçlara dahil etmez (Unity disable
    /// collider'ları scene query'lerinde dikkate almaz). Bu yüzden burada
    /// yalnızca oyuncu collider'larını filtrelememiz yeterli.
    /// </summary>
    private bool IsPoseValid(IPlaceable placeable)
    {
        foreach (var col in placeable.PlacementColliders)
        {
            if (col == null) continue;

            Collider[] overlaps = Physics.OverlapBox(
                col.bounds.center, col.bounds.extents, col.transform.rotation,
                obstructionLayerMask, QueryTriggerInteraction.Ignore);

            foreach (var overlap in overlaps)
            {
                if (System.Array.IndexOf(playerIgnoreColliders, overlap) >= 0) continue;
                return false;
            }
        }
        return true;
    }

    private bool TryGetLookedAtPlaceable(out IPlaceable placeable)
    {
        placeable = null;
        if (mainCamera == null) return false;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, detectDistance, obstructionLayerMask)) return false;
        if (!hit.collider.TryGetComponent(out IPlaceable candidate)) return false;

        // BasePickableItem'lar zaten Case A (elde tutma) akışıyla yerleştirilir.
        if (candidate is BasePickableItem) return false;

        placeable = candidate;
        return true;
    }

    /// <summary>
    /// Placement moduna girerken orijinal materyalleri saklar; bu sayede
    /// iptal/onay sonrası nesnenin görünümü sorunsuz eski haline döner.
    /// </summary>
    private void CacheOriginalMaterials(IPlaceable target)
    {
        originalMaterials.Clear();
        foreach (var renderer in target.PlacementRenderers)
        {
            if (renderer == null) continue;
            originalMaterials[renderer] = renderer.sharedMaterials;
        }

        // Placement moduna girişte henüz geçerlilik hesaplanmadığı için
        // varsayılan olarak "geçersiz" görünümüyle başlıyoruz.
        isCurrentPoseValid = false;
        ApplyPreviewMaterial(false);
    }

    private void ApplyPreviewMaterial(bool isValid)
    {
        Material feedbackMat = isValid ? placeableMaterial : notPlaceableMaterial;
        if (feedbackMat == null || activePlaceable == null) return;

        foreach (var renderer in activePlaceable.PlacementRenderers)
        {
            if (renderer == null || !originalMaterials.TryGetValue(renderer, out var original)) continue;

            var replaced = new Material[original.Length];
            for (int i = 0; i < replaced.Length; i++)
                replaced[i] = feedbackMat;

            renderer.materials = replaced;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null) kvp.Key.materials = kvp.Value;
        }
        originalMaterials.Clear();
    }
}