using System;
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
    public bool HasDetection => DetectedPickable != null || DetectedInteractable != null;
    public event Action<bool> OnShouldCheckInteractionStateChanged;
    public event Action<BasePickableItem, IInteractable> OnDetectionChanged;

    BasePickableItem prevPickable;
    IInteractable prevInteractable;
    
    private int raycastMask;
    private int outlineLayer;

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
        raycastMask = interactableLayer.value;
        outlineLayer = LayerMask.NameToLayer("Outline");
        if (!mainCam) Debug.LogError("[PlayerInteraction] Ana kamera bulunamadı!");

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

        int interactableLayerValue = LayerMask.NameToLayer("Interactable");
        if (interactableLayerValue >= 0)
        {
            if (DetectedPickable != null)
                FunctionLibrary.SetLayerRecursively(DetectedPickable.transform, interactableLayerValue);

            if (DetectedInteractable is MonoBehaviour detectedInteractableBehaviour)
                FunctionLibrary.SetLayerRecursively(detectedInteractableBehaviour.transform, interactableLayerValue);
        }

        DetectedPickable = null;
        DetectedInteractable = null;
        OnDetectionChanged?.Invoke(null, null);
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

        DetectedPickable = null;
        DetectedInteractable = null;

        if (mainCam)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (outlineLayer >= 0)
            {
                raycastMask |= 1 << outlineLayer;
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
            }
        }

        HandleOutline(DetectedPickable, DetectedInteractable);

        if (prevPickable != DetectedPickable || prevInteractable != DetectedInteractable)
            OnDetectionChanged?.Invoke(DetectedPickable, DetectedInteractable);
    }

    private void HandleOutline(BasePickableItem pickable, IInteractable interactable)
    {
        int interactableLayerValue = LayerMask.NameToLayer("Interactable");
        int outlineLayerValue = LayerMask.NameToLayer("Outline");

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
        if (DetectedPickable != null)
        {
            PlayerItemHolder.Instance.TryPickup(DetectedPickable);
            return;
        }
    }
    
    //DEBUG
    private void OnGUI()
    {
        if (!enableDebug) return;
        
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(10, 10, 300, 120), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };

        float x = 18, y = 14, lineH = 22;

        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x, y, 280, lineH), "── PlayerInteraction Debug ──", style);
        y += lineH;

        // Raycast durumu
        style.normal.textColor = HasDetection ? Color.green : Color.red;
        GUI.Label(new Rect(x, y, 280, lineH),
            $"Raycast: {(HasDetection ? "HIT" : "MISS")}", style);
        y += lineH;

        // IPickable
        style.normal.textColor = DetectedPickable != null ? Color.yellow : Color.gray;
        GUI.Label(new Rect(x, y, 280, lineH),
            $"IPickable:     {(DetectedPickable != null ? DetectedPickable.GetType().Name : "—")}", style);
        y += lineH;

        // IInteractable
        style.normal.textColor = DetectedInteractable != null ? Color.yellow : Color.gray;
        GUI.Label(new Rect(x, y, 280, lineH),
            $"IInteractable: {(DetectedInteractable != null ? DetectedInteractable.GetType().Name : "—")}", style);
        y += lineH;

        // Holding item
        bool holding = PlayerItemHolder.Instance != null && PlayerItemHolder.Instance.IsHoldingItem;
        style.normal.textColor = holding ? Color.magenta : Color.gray;
        GUI.Label(new Rect(x, y, 280, lineH),
            $"Holding Item:  {(holding ? "EVET" : "HAYIR")}", style);
        if (holding)
            GUI.Label(new Rect(x + 150, y, 280, lineH),
                $"| {PlayerItemHolder.Instance.CurrentItem.ToString()}", style);
    }
}