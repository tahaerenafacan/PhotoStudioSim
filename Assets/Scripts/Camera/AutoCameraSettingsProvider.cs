using UnityEngine;

namespace SyntaxSultan.CameraSystem
{
    /// <summary>
    /// Hedef EV100'e göre shutter hızını otomatik hesaplar.
    /// Geniş aperture (f/2.8) sabit tutulur; bokeh efekti güçlü kalır.
    /// </summary>
    public class AutoCameraSettingsProvider : ICameraSettingsProvider
    {
        private const float TargetEV100     = 10f;  // Indoor referans
        private const float AutoAperture    = 2.8f;
        private const float AutoISO         = 400f;

        private float focalLength;

        public AutoCameraSettingsProvider(float initialFocalLength)
            => focalLength = initialFocalLength;

        public void SetFocalLength(float fl) => focalLength = fl;

        public CameraPhysicalSettings GetSettings()
        {
            // EV100 = log₂(N²/t) − log₂(ISO/100) → t = N² / 2^(EV100+log₂(ISO/100))
            float ev      = TargetEV100 + Mathf.Log(AutoISO / 100f, 2f);
            float shutter = (AutoAperture * AutoAperture) / Mathf.Pow(2f, ev);

            return new CameraPhysicalSettings
            {
                ISO          = AutoISO,
                Aperture     = AutoAperture,
                ShutterSpeed = Mathf.Clamp(shutter, 1f / 4000f, 1f / 30f),
                FocalLength  = focalLength
            };
        }
    }
}