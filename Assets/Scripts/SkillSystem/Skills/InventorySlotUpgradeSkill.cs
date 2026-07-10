using SyntaxSultan.InventoryModule;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventorySlotSkill", menuName = "PSSGame/Skills/Inventory Slot Skill")]
public class InventorySlotUpgradeSkill : SkillDefinition
{
    [SerializeField] private int slotsToAdd = 1;

    public override void ApplySkill()
    {
        var inventory = InventorySystem.Instance;
        if (inventory == null) return;

        inventory.UpgradeSlotCount(inventory.SlotCount + slotsToAdd);
    }
}