using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SyntaxSultan.SavingSystem;
using UnityEngine;

public class SkillManager : MonoBehaviour, IJsonSaveable
{
    public static SkillManager Instance { get; private set; }

    [Tooltip("Oyuncunun açabileceği tüm skill'ler (UI listelemek için).")]
    [SerializeField] private List<SkillDefinition> availableSkills = new();

    private readonly HashSet<SkillDefinition> unlockedSkills = new();

    public int AvailableSkillPoints { get; private set; }
    public IReadOnlyList<SkillDefinition> AvailableSkills => availableSkills;

    public event Action<int> OnSkillPointsChanged;
    public event Action<SkillDefinition> OnSkillUnlocked;

    private const string SkillPointsKey = "availableSkillPoints";
    private const string UnlockedSkillsKey = "unlockedSkills";

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

    public JToken CaptureAsJToken()
    {
        JObject state = new JObject();
        state[SkillPointsKey] = AvailableSkillPoints;

        JArray unlockedSkillNames = new JArray();
        foreach (SkillDefinition skill in unlockedSkills)
        {
            if (skill != null)
                unlockedSkillNames.Add(skill.name);
        }

        state[UnlockedSkillsKey] = unlockedSkillNames;
        return state;
    }

    public void RestoreFromJToken(JToken state)
    {
        if (state == null || state.Type != JTokenType.Object)
            return;

        JObject stateObject = state.ToObject<JObject>();
        AvailableSkillPoints = stateObject[SkillPointsKey]?.ToObject<int>() ?? AvailableSkillPoints;

        unlockedSkills.Clear();
        JArray unlockedSkillNames = stateObject[UnlockedSkillsKey] as JArray;
        if (unlockedSkillNames != null)
        {
            foreach (JToken skillNameToken in unlockedSkillNames)
            {
                string skillName = skillNameToken.ToObject<string>();
                if (string.IsNullOrEmpty(skillName))
                    continue;

                SkillDefinition skill = availableSkills.Find(x => x != null && x.name == skillName);
                if (skill != null)
                {
                    unlockedSkills.Add(skill);
                    skill.ApplySkill();
                }
                else
                {
                    Debug.LogWarning($"SkillManager: saved skill '{skillName}' not found in availableSkills.");
                }
            }
        }

        OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
    }
}