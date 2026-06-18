using UnityEngine;

namespace SyntaxSultan.CameraSystem
{
    /// <summary>
/// Gerçek kamera diyallarını simüle eder: adımlı (stop) değerler.
/// CycleActiveParameter ile hangi parametrenin scroll'dan etkileneceği seçilir.
/// </summary>
public class ManualCameraSettingsProvider : ICameraSettingsProvider
{
    public static readonly float[] ISOStops = { 100f, 200f, 400f, 800f, 1600f, 3200f, 6400f };
    public static readonly float[] ApertureStops = { 1.4f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f };
    public static readonly float[] ShutterStops =
        { 1f/4000f, 1f/2000f, 1f/1000f, 1f/500f, 1f/250f, 1f/125f, 1f/60f, 1f/30f };
    public static readonly string[] ShutterLabels =
        { "1/4000", "1/2000", "1/1000", "1/500", "1/250", "1/125", "1/60", "1/30" };

    private int isoIndex      = 2;  // 400
    private int apertureIndex = 2;  // f/2.8
    private int shutterIndex  = 4;  // 1/250
    private float focalLength;

    public ManualParameter ActiveParameter { get; private set; } = ManualParameter.ISO;
    public int ISOIndex      => isoIndex;
    public int ApertureIndex => apertureIndex;
    public int ShutterIndex  => shutterIndex;

    public ManualCameraSettingsProvider(float initialFocalLength)
        => focalLength = initialFocalLength;

    public void SetFocalLength(float fl) => focalLength = fl;

    /// <summary>
    /// Aktif parametreyi bir stop yukarı/aşağı kaydırır.
    /// Scroll yönü: +1 = değer artır, -1 = değer azalt.
    /// </summary>
    public void StepActiveParameter(int direction)
    {
        switch (ActiveParameter)
        {
            case ManualParameter.ISO:
                isoIndex = Mathf.Clamp(isoIndex + direction, 0, ISOStops.Length - 1);
                break;
            case ManualParameter.Aperture:
                apertureIndex = Mathf.Clamp(apertureIndex + direction, 0, ApertureStops.Length - 1);
                break;
            case ManualParameter.ShutterSpeed:
                shutterIndex = Mathf.Clamp(shutterIndex + direction, 0, ShutterStops.Length - 1);
                break;
        }
    }

    public void CycleActiveParameter()
        => ActiveParameter = (ManualParameter)(((int)ActiveParameter + 1) % 3);

    public CameraPhysicalSettings GetSettings() => new()
    {
        ISO          = ISOStops[isoIndex],
        Aperture     = ApertureStops[apertureIndex],
        ShutterSpeed = ShutterStops[shutterIndex],
        FocalLength  = focalLength
    };
}
}