using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Tooltip("Oyuncunun açabileceği tüm skill'ler (UI listelemek için).")]
    [SerializeField] private List<SkillDefinition> availableSkills = new();

    private readonly HashSet<SkillDefinition> unlockedSkills = new();

    public int AvailableSkillPoints { get; private set; }
    public IReadOnlyList<SkillDefinition> AvailableSkills => availableSkills;

    public event Action<int> OnSkillPointsChanged;
    public event Action<SkillDefinition> OnSkillUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnReputationLevelUp += HandleReputationLevelUp;
    }

    private void OnDestroy()
    {
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnReputationLevelUp -= HandleReputationLevelUp;
    }

    private void HandleReputationLevelUp(int newLevel) => AddSkillPoints(1);

    public void AddSkillPoints(int amount)
    {
        AvailableSkillPoints += amount;
        OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
    }

    public bool IsUnlocked(SkillDefinition skill) => unlockedSkills.Contains(skill);

    public bool CanUnlock(SkillDefinition skill)
    {
        if (skill == null || IsUnlocked(skill) || AvailableSkillPoints < skill.Cost)
            return false;

        foreach (var prerequisite in skill.Prerequisites)
        {
            if (!IsUnlocked(prerequisite)) return false;
        }
        return true;
    }

    public bool TryUnlockSkill(SkillDefinition skill)
    {
        if (!CanUnlock(skill)) return false;

        AvailableSkillPoints -= skill.Cost;
        unlockedSkills.Add(skill);
        skill.ApplySkill();

        OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
        OnSkillUnlocked?.Invoke(skill);
        return true;
    }

}