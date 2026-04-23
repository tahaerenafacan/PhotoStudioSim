using UnityEngine.Localization;

public interface IInteractable
{
    /// <summary>
    /// Crosshair'de "E: ___" kısmında gösterilecek metin.
    /// </summary>
    LocalizedString InteractHint { get; }

    /// <summary>Bu nesne şu anda etkileşime açık mı?</summary>
    bool CanInteract { get; }

    /// <summary>Oyuncu E tuşuna bastığında çağrılır.</summary>
    void Interact();
}