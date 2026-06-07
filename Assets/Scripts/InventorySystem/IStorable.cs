using UnityEngine;

namespace SyntaxSultan.InventoryModule
{
    /// <summary>
    /// Envanterde saklanabilen itemların implement etmesi gereken interface.
    ///
    /// KULLANIM:
    ///   Mevcut BasePickableItem subclass'ına ekle:
    ///   public class KeyItem : BasePickableItem, IStorable { ... }
    ///
    ///   CanStore = false ise InventoryController item'ı slota koymaz,
    ///   oyuncu elde tutmaya devam eder.
    /// </summary>
    public interface IStorable
    {
        /// <summary>Bu item şu an envantere konulabilir mi? (Runtime değişebilir)</summary>
        bool CanStore { get; }

        /// <summary>Slot UI'ında gösterilecek ikon. ItemDefinition.icon döndürmek yeterli.</summary>
        Sprite Icon { get; }
    }
}