using UnityEngine;

namespace SyntaxSultan.CameraSystem
{
    /// <summary>
    /// Fotoğrafçılık EV formülleri.
    /// PostExposure, grain ve motion blur yoğunluğunu normalize eder.
    /// </summary>
    public static class CameraExposure
    {
        // EV100 = log₂(N²/t) − log₂(ISO/100)
        // Indoor referans ≈ 10, güneşli dış ≈ 13
        public static float CalculateEV100(float iso, float aperture, float shutter)
        {
            if (shutter <= 0f || aperture <= 0f) return 0f;
            float ev = Mathf.Log(aperture * aperture / shutter, 2f);
            return ev - Mathf.Log(iso / 100f, 2f);
        }

        // Pozitif = aşık, negatif = karanlık
        public static float GetPostExposureStops(float ev100, float referenceEV = 10f)
            => referenceEV - ev100;

        // ISO 100 → 0.0, ISO 6400 → 1.0
        public static float GetGrainIntensity(float iso)
            => Mathf.InverseLerp(100f, 6400f, iso);

        // 1/4000 → 0.0, 1/30 → 1.0
        public static float GetMotionBlurIntensity(float shutter)
            => Mathf.InverseLerp(1f / 4000f, 1f / 30f, shutter);
    }
}