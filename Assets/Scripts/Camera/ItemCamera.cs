using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class ItemCamera : BasePickableItem, IUsable
{
    public LocalizedString UseHint => Definition.useHint;
    
    [SerializeField] private Canvas screenCanvas;
    
    [Header("Lens")]
    [SerializeField] private Camera lensCamera;
    [Tooltip("Must be same RenderTexture with lensCamera.targetTexture.")]
    [SerializeField] private RenderTexture viewfinderTexture;
    
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private Ease zoomEase = Ease.OutQuad;
    [SerializeField] private float maxFov = 100;
    [SerializeField] private float minFov = 20;
    
    [SerializeField] private Vector3 focusOnCameraLocalPosition;
    private bool wasRightButtonPressed = false;
    private float currentFOV;

    [Header("Photo Settings")]
    [Tooltip("Saved photo resolution.")]
    [SerializeField] private Vector2Int photoResolution = new(256, 256);

    [Tooltip("Max camera storage")]
    [SerializeField] private int maxLocalPhotos = 10;

    [Header("Sound / Effect")]
    [SerializeField] private MMF_Player photoTakenEffects;

    
    [Header("Flash")]
    [SerializeField] private bool useFlash = true;
    [SerializeField] private UnityEngine.UI.Image flashImage;
    [SerializeField] private Sprite flashOnSprite;
    [SerializeField] private Sprite flashOffSprite;

    private readonly List<Texture2D> localPhotos = new();
    private bool isHeld;

    protected override void Awake()
    {
        base.Awake();
        currentFOV = lensCamera.fieldOfView;
    }

    private void Update()
    {
        if (!isHeld) return;
        bool isRightPressed = Mouse.current.rightButton.isPressed;

        if (isRightPressed != wasRightButtonPressed)
        {
            SetLocalPosition(isRightPressed ? focusOnCameraLocalPosition : Definition.holdPositionOffset);
            wasRightButtonPressed = isRightPressed;
        }

        //TODO: Import photos from computer
        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            TransferToComputer();
        }
        
        if (Keyboard.current.numpadMultiplyKey.wasPressedThisFrame)
        {
            ToggleFlash();
        }
        
        
        //Zoom In/Out
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0f)
        {
            float targetFOV = Mathf.Clamp(
                currentFOV - scroll * zoomSpeed * (maxFov - minFov),
                minFov,
                maxFov
            );

            currentFOV = targetFOV;

            lensCamera.DOKill();
            lensCamera.DOFieldOfView(targetFOV, 0.25f).SetEase(zoomEase);
        }
    }
    
    public void TransferToComputer()
    {
        if (localPhotos.Count == 0) return;
        

        CameraStorage.Instance.Upload(localPhotos);
        //int count = localPhotos.Count;
        localPhotos.Clear();
    }

    public void OnUseStart()
    {
        if (!isHeld) return;
        TakePhoto();
    }

    public void OnUseStop()
    {
        
    }
    
    protected override void OnPickedUp()
    {
        isHeld = true;
        lensCamera.enabled = true;
        screenCanvas.gameObject.SetActive(true);
    }

    protected override void OnDropped()
    {
        isHeld = false;
        wasRightButtonPressed = false; 
        lensCamera.enabled = false;
        screenCanvas.gameObject.SetActive(false);
    }

    private void ToggleFlash()
    {
        useFlash = !useFlash;
        flashImage.sprite = useFlash ? flashOnSprite : flashOffSprite;
    }
    
    private void TakePhoto()
    {
        if (localPhotos.Count >= maxLocalPhotos) return;
        
        if (viewfinderTexture == null)
        {
            Debug.LogError("[GameCamera] viewfinderTexture atanmamış!");
            return;
        }

        Texture2D snap = new Texture2D(photoResolution.x, photoResolution.y, TextureFormat.RGB24, false);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = viewfinderTexture;

        snap.ReadPixels(new Rect(0, 0, viewfinderTexture.width, viewfinderTexture.height), 0, 0);
        snap.Apply();
        
        RenderTexture.active = prev;
        
        SaveAsPNG(snap);

        localPhotos.Add(snap);

        photoTakenEffects.FeedbacksList[1].Active = useFlash;
        photoTakenEffects.PlayFeedbacks();
    }

    private void SaveAsPNG(Texture2D textureToSave)
    {
        byte[] bytes = textureToSave.EncodeToPNG();

        string folderPath = Path.Combine(Application.persistentDataPath, "SavedPhotos");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, $"Photo_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        try
        {
            File.WriteAllBytes(filePath, bytes);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }
}