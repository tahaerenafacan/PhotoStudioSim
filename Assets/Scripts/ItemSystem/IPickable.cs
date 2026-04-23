using UnityEngine;

/// <summary>
/// BasePickableItem tarafından implement edilen, yerden alınabilen itemlerin sahip olması gereken temel fonksiyonları tanımlar.
/// </summary>
public interface IPickable
{
    /// <summary>Oyuncu itemi eline aldığında çağrılır.</summary>
    void OnPickup(Transform holdPoint);

    /// <summary>Oyuncu itemi bıraktığında/fırlattığında çağrılır.</summary>
    void OnDrop(Vector3 throwDirection, float throwForce);
}