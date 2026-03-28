/// <summary>
/// Elde tutulurken sol tıkla kullanılabilen itemler bu arayüzü implement eder.
/// Örn: Kamera (fotoğraf çek), Süpürge (süpür), Silah (ateş et)
/// </summary>
public interface IUsable
{
    /// <summary>
    /// Ekranda gösterilecek kullanım ipucu. 
    /// Örn: "Sol Tık: Fotoğraf Çek" veya "Sol Tık [Basılı Tut]: Süpür"
    /// </summary>
    string UseHint { get; }

    /// <summary>Sol tık basıldığında çağrılır.</summary>
    void OnUseStart();

    /// <summary>Sol tık bırakıldığında çağrılır.</summary>
    void OnUseStop();
}