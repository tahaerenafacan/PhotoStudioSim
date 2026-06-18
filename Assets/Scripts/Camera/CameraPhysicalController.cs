using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SyntaxSultan.CameraSystem
{
    /// <summary>
    /// ICameraSettingsProvider'dan gelen ayarları Camera + URP Volume'a yansıtır.
    /// ApplyPostProcess: sadece volume efektleri (her frame çağrılabilir, cheap).
    /// SetFocalLength: sadece odak uzunluğu (tween ile ItemCamera yönetir).
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraPhysicalController : MonoBehaviour
    {
        [Header("Sensor – Full-frame 35mm")] [SerializeField]
        private Vector2 sensorSize = new(36f, 24f);

        [Header("Post Process Volume")] [SerializeField]
        private Volume postProcessVolume;

        [Tooltip("Lens kameranın göreceği Volume layer — Inspector'da Volume'un layer'ıyla eşleşmeli.")]
        [SerializeField]
        private LayerMask volumeLayerMask = 1; // Default layer

        [Header("Auto Focus")] [SerializeField]
        private bool enableAutoFocus = true;

        [SerializeField] private LayerMask focusLayerMask = ~0;
        [SerializeField] private float focusLerpSpeed = 5f;

        private Camera lensCamera;
        private DepthOfField dof;
        private MotionBlur motionBlur;
        private ColorAdjustments colorAdjustments;
        private FilmGrain filmGrain;

        private float currentFocusDistance = 5f;

        private void Awake()
        {
            lensCamera = GetComponent<Camera>();
            lensCamera.usePhysicalProperties = true;
            lensCamera.sensorSize = sensorSize;

            var cameraData = lensCamera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.volumeLayerMask      = volumeLayerMask;

            if (postProcessVolume == null) return;

            // Orijinal .asset'i kirletmemek için runtime kopyası al
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);

            postProcessVolume.profile.TryGet(out dof);
            postProcessVolume.profile.TryGet(out motionBlur);
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out filmGrain);
        }

        private void Update()
        {
            if (enableAutoFocus && lensCamera.enabled)
                TickAutoFocus();
        }

        // ── Public API ─────────────────────────────────────────────

        public void SetFocalLength(float focalLength)
        {
            lensCamera.focalLength = focalLength;

            if (dof != null)
                dof.focalLength.value = focalLength;
        }

        /// <summary>
        /// Volume efektlerini uygular; focal length'e dokunmaz (tween çakışmasını önler).
        /// </summary>
        public void ApplyPostProcess(CameraPhysicalSettings s)
        {
            ApplyDepthOfField(s);
            ApplyMotionBlur(s);
            ApplyExposure(s);
            ApplyFilmGrain(s);
        }

        // ── Private Helpers ────────────────────────────────────────

        private void ApplyDepthOfField(CameraPhysicalSettings s)
        {
            if (dof == null) return;

            dof.active = true;
            dof.mode.overrideState         = true; dof.mode.value         = DepthOfFieldMode.Bokeh;
            dof.aperture.overrideState     = true; dof.aperture.value     = s.Aperture;
            dof.focalLength.overrideState  = true; dof.focalLength.value  = lensCamera.focalLength;
            dof.focusDistance.overrideState = true; dof.focusDistance.value = Mathf.Max(0.1f, currentFocusDistance);
        }

        private void ApplyMotionBlur(CameraPhysicalSettings s)
        {
            if (motionBlur == null) return;

            float intensity = CameraExposure.GetMotionBlurIntensity(s.ShutterSpeed);
            motionBlur.active                = intensity > 0.02f;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value         = intensity * 0.5f;
        }

        private void ApplyExposure(CameraPhysicalSettings s)
        {
            if (colorAdjustments == null) return;

            float ev100 = CameraExposure.CalculateEV100(s.ISO, s.Aperture, s.ShutterSpeed);
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value         = CameraExposure.GetPostExposureStops(ev100);
        }

        private void ApplyFilmGrain(CameraPhysicalSettings s)
        {
            if (filmGrain == null) return;

            filmGrain.intensity.overrideState = true;
            filmGrain.intensity.value         = CameraExposure.GetGrainIntensity(s.ISO) * 0.4f;
        }

        /// <summary>
        /// Kameranın önündeki yüzeye raycast ile smooth odak mesafesi hesaplar.
        /// </summary>
        private void TickAutoFocus()
        {
            if (Physics.Raycast(lensCamera.transform.position,
                    lensCamera.transform.forward,
                    out RaycastHit hit, 50f, focusLayerMask))
            {
                currentFocusDistance = Mathf.Lerp(currentFocusDistance, hit.distance,
                    Time.deltaTime * focusLerpSpeed);
            }
        }
        
        /// <summary>
        /// Volume pipeline garantisi olmaksızın efektleri doğrudan texture piksellerine yazar.
        /// Çağrı sırası: ReadPixels → Apply → BakeOntoCapture → SaveAsPNG.
        /// Exposure + grain baked; DOF/MotionBlur RT'de zaten mevcutsa çift uygulanmaz.
        /// </summary>
        public void BakeOntoCapture(Texture2D tex, CameraPhysicalSettings s)
        {
            Color[] pixels = tex.GetPixels();

            float ev100      = CameraExposure.CalculateEV100(s.ISO, s.Aperture, s.ShutterSpeed);
            float exposure   = Mathf.Pow(2f, CameraExposure.GetPostExposureStops(ev100));
            float grainPower = CameraExposure.GetGrainIntensity(s.ISO) * 0.06f;

            // Volume'dan gelen postExposure'u normalize etmek için:
            // RT'de zaten uygulandıysa exposure ≈ 1 bırak, uygulanmadıysa devreye girer.
            bool volumeWorking = colorAdjustments != null && colorAdjustments.active;
            float exposureMultiplier = volumeWorking ? 1f : exposure;

            bool filmGrainWorking = filmGrain != null && filmGrain.active;
            float grainMultiplier = filmGrainWorking ? 0f : grainPower; // çift grain engeli

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i] * exposureMultiplier;

                // Luminance-weighted grain: gölgelerde ISO patlaması daha görünür
                if (grainMultiplier > 0f)
                {
                    float lum   = 1f - c.grayscale;
                    float noise = (UnityEngine.Random.value * 2f - 1f) * grainMultiplier * lum;
                    c.r = Mathf.Clamp01(c.r + noise);
                    c.g = Mathf.Clamp01(c.g + noise);
                    c.b = Mathf.Clamp01(c.b + noise);
                }

                pixels[i] = c;
            }

            tex.SetPixels(pixels);
            tex.Apply();
        }
    }
}