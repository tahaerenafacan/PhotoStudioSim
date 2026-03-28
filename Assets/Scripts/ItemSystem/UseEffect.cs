using UnityEngine;

/// <summary>
/// Kullanılabilir item efektlerinin soyut temeli.
/// Yeni efekt eklemek için: bu sınıfı extend et, Apply() override et,
/// CreateAssetMenu ile asset yarat, ItemDefinition'ın effect slotuna sürükle.
/// </summary>
public abstract class UseEffect : ScriptableObject
{
    public bool consumeOnUse = false;

    /// <summary>
    /// PlayerItemHolder.Use() tarafından çağrılır.
    /// player: PlayerItemHolder'ın bulunduğu GameObject.
    /// </summary>
    public abstract void Apply(GameObject player);
}