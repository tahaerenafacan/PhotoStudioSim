using UnityEngine;

[CreateAssetMenu(fileName = "NewMovementSpeedSkill", menuName = "PSSGame/Skills/Movement Speed Skill")]
public class MovementSpeedSkill : SkillDefinition
{
    [SerializeField] private float speedIncrease = 1f;

    public override void ApplySkill()
    {
        // ScriptableObject sahne referansı tutmamalı; sahnedeki instance runtime'da bulunur.
        var player = FindFirstObjectByType<PlayerCharacter>();
        //player?.IncreaseBaseSpeed(speedIncrease);
    }
}