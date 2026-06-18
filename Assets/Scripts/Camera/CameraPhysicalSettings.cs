namespace SyntaxSultan.CameraSystem
{
    public enum CameraMode { Auto, Manual }
    public enum ManualParameter { ISO, Aperture, ShutterSpeed }

    /// <summary>
    /// Tek veri kaynağı: tüm fiziksel kamera parametreleri.
    /// ICameraSettingsProvider → ApplySettings pipeline'ında taşınır.
    /// </summary>
    [System.Serializable]
    public struct CameraPhysicalSettings
    {
        public float ISO;           // 100–6400
        public float Aperture;      // f-number: 1.4–16
        public float ShutterSpeed;  // saniye: 1/4000–1/30
        public float FocalLength;   // mm

        public static CameraPhysicalSettings Default => new()
        {
            ISO          = 400f,
            Aperture     = 2.8f,
            ShutterSpeed = 1f / 250f,
            FocalLength  = 50f
        };
    }
}