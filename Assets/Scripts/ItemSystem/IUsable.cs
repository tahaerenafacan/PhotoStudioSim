using UnityEngine.Localization;

/// <summary>
/// Elde tutulurken sol tıkla kullanılabilen itemler bu arayüzü implement eder.
/// Örn: Kamera (fotoğraf çek), Süpürge (süpür), Silah (ateş et)
/// </summary>
public interface IUsable
{
    /// <summary>
    /// Ekranda gösterilecek kullanım ipucu. 
    /// </summary>
    LocalizedString UseHint { get; }

    /// <summary>Sol tık basıldığında çağrılır.</summary>
    void OnUseStart();

    /// <summary>Sol tık bırakıldığında çağrılır.</summary>
    void OnUseStop();
}