using UnityEngine;

/// <summary>
/// Yerden alınabilen her item bu arayüzü implement eder.
/// </summary>
public interface IPickable
{
    /// <summary>Oyuncu itemi eline aldığında çağrılır.</summary>
    void OnPickup(Transform holdPoint);

    /// <summary>Oyuncu itemi bıraktığında/fırlattığında çağrılır.</summary>
    void OnDrop(Vector3 throwDirection, float throwForce);
}