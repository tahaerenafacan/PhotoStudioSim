/// <summary>
/// E tuşuyla etkileşim kurulabilen her obje bu arayüzü implement eder.
/// Sadece interact: kapı, düğme, NPC vb.
/// Hem pickup hem interact: alınabilir ama aynı zamanda etkileşimli objeler.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Crosshair'de "E: ___" kısmında gösterilecek metin.
    /// Örn: "Aç", "Konuş", "Kullan"
    /// </summary>
    string InteractHint { get; }

    /// <summary>Oyuncu E tuşuna bastığında çağrılır.</summary>
    void Interact();
}