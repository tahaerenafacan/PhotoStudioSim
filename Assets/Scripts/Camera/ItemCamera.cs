using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using MoreMountains.Feedbacks;
using SyntaxSultan.InventoryModule;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace SyntaxSultan.CameraSystem
{
    public class ItemCamera : BasePickableItem, IComplexUsable, IStorable
    {
        [Header("Lens")] 
        [SerializeField] private Camera lensCamera;

        [SerializeField, Tooltip("Must be same RenderTexture with lensCamera.targetTexture.")] private RenderTexture viewfinderTexture;

        [SerializeField] private CameraScreen cameraScreen;
        [SerializeField] private CameraPhysicalController physicalController;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 0.2f;
        [SerializeField] private Ease zoomEase = Ease.OutQuad;
        [SerializeField] private float minFocalLength = 24f;
        [SerializeField] private float maxFocalLength = 200f;
        [SerializeField] private float normalFocalLength = 50f;

        [SerializeField] private Vector3 focusOnCameraLocalPosition;

        [Header("Photo Settings")] [Tooltip("Saved photo resolution.")] 
        [SerializeField] private Vector2Int photoResolution = new(256, 256);

        [Tooltip("Max camera storage")] 
        [SerializeField] private int maxLocalPhotos = 10;

        [Header("Sound / Effect")] 
        [SerializeField] private MMF_Player photoTakenEffects;

        [Header("Input References")] 
        [SerializeField] private InputActionReference shootAction;
        [SerializeField] private InputActionReference toggleModeAction;
        [SerializeField] private InputActionReference cycleParamAction;

        [SerializeField] private InputActionReference focusAction;
        [SerializeField] private InputActionReference toggleFlashAction;
        [SerializeField] private InputActionReference uploadPhotosAction;

        [Header("Localization Hints")] 
        [SerializeField] private LocalizedString shootHint;

        [SerializeField] private LocalizedString focusHint;
        [SerializeField] private LocalizedString toggleFlashHint;
        [SerializeField] private LocalizedString uploadPhotosHint;
        [SerializeField] private LocalizedString toggleModeHint;
        [SerializeField] private LocalizedString cycleParamHint;
        
        private bool useFlash = true;
        private bool wasRightButtonPressed;
        private readonly List<Texture2D> localPhotos = new();
        private bool isHeld;
        
        //Pysical Cam Settings
        private float currentFocalLength;
        private CameraMode currentMode = CameraMode.Auto;
        private AutoCameraSettingsProvider autoProvider;
        private ManualCameraSettingsProvider manualProvider;
        private ICameraSettingsProvider activeProvider;

        private List<ItemInteraction> interactions;

        protected override void Awake()
        {
            base.Awake();
            InitializeInteractions();
            currentFocalLength = normalFocalLength;
            autoProvider  = new AutoCameraSettingsProvider(normalFocalLength);
            manualProvider = new ManualCameraSettingsProvider(normalFocalLength);
            activeProvider = autoProvider;
        }

        private void InitializeInteractions()
        {
            interactions = new List<ItemInteraction>();

            // 1. Fotoğraf Çekme Etkileşimi
            var shootInteract = new ItemInteraction(shootAction, shootHint);
            shootInteract.OnPerformed += ctx => TakePhoto();
            interactions.Add(shootInteract);

            // 2. Ayar Değiştirme Etkileşimi
            var settingsInteract = new ItemInteraction(toggleFlashAction, toggleFlashHint);
            settingsInteract.OnPerformed += ctx => ToggleFlash();
            interactions.Add(settingsInteract);

            // 3. Fotoğraf Silme Etkileşimi
            var deleteInteract = new ItemInteraction(uploadPhotosAction, uploadPhotosHint);
            deleteInteract.OnPerformed += ctx => TransferToComputer();
            interactions.Add(deleteInteract);

            var focusInteract = new ItemInteraction(focusAction, focusHint);
            focusInteract.OnPerformed += ctx => HandleCameraFocus();
            interactions.Add(focusInteract);
            
            var toggleModeInteract = new ItemInteraction(toggleModeAction, toggleModeHint);
            toggleModeInteract.OnPerformed += _ => ToggleCameraMode();
            interactions.Add(toggleModeInteract);

            var cycleParamInteract = new ItemInteraction(cycleParamAction, cycleParamHint);
            cycleParamInteract.OnPerformed += _ => CycleManualParameter();
            interactions.Add(cycleParamInteract);
        }

        private void Update()
        {
            if (!isHeld) return;
            HandleZoomInput();
            ApplyAndDisplaySettings();
        }

        private void HandleCameraFocus()
        {
            wasRightButtonPressed = !wasRightButtonPressed;
            if (wasRightButtonPressed)
            {
                PlayerInteraction.Instance.DisableInteraction();
                SetLocalPosition(focusOnCameraLocalPosition);
            }
            else
            {
                PlayerInteraction.Instance.EnableInteraction();
                SetLocalPosition(ItemData.holdPositionOffset);
            }
        }

        private void HandleZoomInput()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll == 0f) return;

            // Odak modunda + manuel → scroll aktif parametreyi ayarlar
            if (currentMode == CameraMode.Manual && wasRightButtonPressed)
            {
                int direction = scroll > 0f ? -1 : 1;
                manualProvider.StepActiveParameter(direction);
                return;
            }

            // Normal mod → zoom (focal length)
            float target = Mathf.Clamp(
                currentFocalLength + scroll * zoomSpeed * (maxFocalLength - minFocalLength),
                minFocalLength, maxFocalLength
            );
            currentFocalLength = target;
            autoProvider.SetFocalLength(currentFocalLength);
            manualProvider.SetFocalLength(currentFocalLength);

            // Tween: ItemCamera lensCamera'ya doğrudan erişiyor (PhysicalController ile çakışmasın)
            lensCamera.DOKill();
            DOTween.To(
                () => lensCamera.focalLength,
                x  => physicalController.SetFocalLength(x),
                target, 0.25f
            ).SetEase(zoomEase);
        }

        public void TransferToComputer()
        {
            if (localPhotos.Count == 0) return;

            CameraStorage.Instance.Upload(localPhotos);
            //int count = localPhotos.Count;
            localPhotos.Clear();
        }

        protected override void OnPickedUp()
        {
            isHeld = true;
            lensCamera.enabled = true;
            cameraScreen.SetScreenActive(true);
            ApplyAndDisplaySettings();
        }

        protected override void OnDropped()
        {
            isHeld = false;
            wasRightButtonPressed = false;
            lensCamera.enabled = false;
            cameraScreen.SetScreenActive(false);
        }

        private void ToggleFlash()
        {
            useFlash = !useFlash;
            cameraScreen.SetFlashImage(useFlash);
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
            physicalController.BakeOntoCapture(snap, activeProvider.GetSettings());
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

            string filePath = Path.Combine(folderPath, $"Photo_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            try
            {
                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }
        
        private void ToggleCameraMode()
        {
            currentMode    = currentMode == CameraMode.Auto ? CameraMode.Manual : CameraMode.Auto;
            activeProvider = currentMode == CameraMode.Auto
                ? (ICameraSettingsProvider)autoProvider
                : manualProvider;
        }

        private void CycleManualParameter()
        {
            if (currentMode == CameraMode.Manual)
                manualProvider.CycleActiveParameter();
        }

        /// <summary>
        /// Post-process ayarlarını uygular ve HUD'ı günceller.
        /// Her Update frame'inde çağrılır; focal length tweeni ile çakışmaz.
        /// </summary>
        private void ApplyAndDisplaySettings()
        {
            CameraPhysicalSettings settings = activeProvider.GetSettings();
            physicalController.ApplyPostProcess(settings);

            float ev100      = CameraExposure.CalculateEV100(settings.ISO, settings.Aperture, settings.ShutterSpeed);
            float zoomRatio  = currentFocalLength / normalFocalLength;
            ManualParameter? activeParam = currentMode == CameraMode.Manual
                ? manualProvider.ActiveParameter
                : (ManualParameter?)null;

            cameraScreen.UpdateZoomDisplay(zoomRatio);
            cameraScreen.UpdatePhysicalDisplay(settings, currentMode, ev100, activeParam);
        }

        // IComplexUseable
        public bool CanStore => true;
        public Sprite Icon => ItemData.icon;

        public List<ItemInteraction> GetInteractions()
        {
            return interactions;
        }
    }
}