using System;
using SyntaxSultan.DirtSystem;
using UnityEngine;

/// <summary>
/// TEŞHİS ÖNCELIK SIRASI (aynı objede ikisi varsa):
///   1) Elde item yoksa + IPickable varsa → item al
///   2) Elde item varsa + IInteractable varsa → interact et
///   3) Elde item yoksa + sadece IInteractable varsa → interact et
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("Raycast Ayarları")]
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    private bool shouldCheckInteraction = true;
    private Camera mainCam;
    
    //---------Public API-----------
    
    public BasePickableItem DetectedPickable { get; private set; }
    public IInteractable DetectedInteractable { get; private set; }
    public ICleanable DetectedCleanable { get; private set; }
    
    public bool HasDetection => DetectedPickable != null || DetectedInteractable != null;
    public event Action<bool> OnShouldCheckInteractionStateChanged;
    public event Action<BasePickableItem, IInteractable> OnDetectionChanged;
    public event Action<ICleanable> OnCleanableChanged;

    BasePickableItem prevPickable;
    IInteractable prevInteractable;
    ICleanable prevCleanable;
    
    private int raycastMask;
    private int outlineLayerValue;
    private int interactableLayerValue;

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
        if (!mainCam) Debug.LogError("[PlayerInteraction] Ana kamera bulunamadı!");
        
        raycastMask = interactableLayer.value;
        outlineLayerValue =      LayerMask.NameToLayer("Outline");
        interactableLayerValue = LayerMask.NameToLayer("Interactable");

        InputManager.Instance.OnInteractKeyPressed += HandleInteractInput;
        InputManager.Instance.OnPickupKeyPressed += HandlePickup;
    }

    private void Update()
    {
        if (!shouldCheckInteraction) return;
        PerformRaycast();
    }

    public void DisableInteraction()
    {
        shouldCheckInteraction = false;
        
        if (interactableLayerValue >= 0)
        {
            if (DetectedPickable != null)
                FunctionLibrary.SetLayerRecursively(DetectedPickable.transform, interactableLayerValue);

            if (DetectedInteractable is MonoBehaviour detectedInteractableBehaviour)
                FunctionLibrary.SetLayerRecursively(detectedInteractableBehaviour.transform, interactableLayerValue);
        }

        DetectedPickable = null;
        DetectedInteractable = null;
        DetectedCleanable = null;
        OnDetectionChanged?.Invoke(null, null);
        OnCleanableChanged?.Invoke(null);
        OnShouldCheckInteractionStateChanged?.Invoke(shouldCheckInteraction);
    }

    public void EnableInteraction()
    {
        shouldCheckInteraction = true;
        OnShouldCheckInteractionStateChanged?.Invoke(shouldCheckInteraction);
    }

    private void PerformRaycast()
    {
        prevPickable = DetectedPickable;
        prevInteractable = DetectedInteractable;
        prevCleanable = DetectedCleanable;

        DetectedPickable = null;
        DetectedInteractable = null;
        DetectedCleanable = null;

        if (mainCam)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (outlineLayerValue >= 0)
            {
                raycastMask |= 1 << outlineLayerValue;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, raycastMask))
            {
                Collider hitCol = hit.collider;

                if (!PlayerItemHolder.Instance.IsHoldingItem)
                {
                    hitCol.TryGetComponent(out BasePickableItem detectedPick);
                    DetectedPickable = detectedPick;
                }

                if (hitCol.TryGetComponent(out IInteractable detectedInteractable))
                {
                    DetectedInteractable = detectedInteractable;
                }
                else
                {
                    DetectedInteractable = null;
                }
                
                if (hitCol.TryGetComponent(out ICleanable detectedCleanable))
                {
                    DetectedCleanable = detectedCleanable;
                }
                else
                {
                    DetectedCleanable = null;
                }
            }
        }

        HandleOutline(DetectedPickable, DetectedInteractable);

        if (prevPickable != DetectedPickable || prevInteractable != DetectedInteractable)
        {
            OnDetectionChanged?.Invoke(DetectedPickable, DetectedInteractable);
        }
        if (prevCleanable != DetectedCleanable && PlayerItemHolder.Instance.CurrentItem is BroomItem)
        {
            OnCleanableChanged?.Invoke(DetectedCleanable);
            Debug.Log("Cleanable");
        }
    }

    private void HandleOutline(BasePickableItem pickable, IInteractable interactable)
    {
        Transform currentTarget = null;

        if (pickable != null)
        {
            currentTarget = pickable.transform;
        }
        else if (interactable is MonoBehaviour interactableBehaviour)
        {
            currentTarget = interactableBehaviour.transform;
        }

        if (prevPickable && prevPickable.transform != currentTarget)
        {
            FunctionLibrary.SetLayerRecursively(prevPickable.transform, interactableLayerValue >= 0 ? interactableLayerValue : 0);
        }

        if (prevInteractable is MonoBehaviour prevInteractableBehaviour && prevInteractableBehaviour && prevInteractableBehaviour.transform != currentTarget)
        {
            FunctionLibrary.SetLayerRecursively(prevInteractableBehaviour.transform, interactableLayerValue >= 0 ? interactableLayerValue : 0);
        }

        if (currentTarget != null && outlineLayerValue >= 0)
        {
            FunctionLibrary.SetLayerRecursively(currentTarget, outlineLayerValue);
        }
    }

    private void HandleInteractInput()
    {
        if (DetectedInteractable != null && DetectedInteractable.CanInteract)
        {
            DetectedInteractable.Interact();
        }
    }

    private void HandlePickup()
    {
        if (DetectedPickable == null) return;
        PlayerItemHolder.Instance.TryPickup(DetectedPickable);
    }
}