using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tüm skill tiplerinin temel tabanı. Yeni bir skill eklemek için bunu extend et
/// ve ApplySkill() içinde efekti tanımla — SkillManager'a dokunmana gerek kalmaz (OCP).
/// </summary>
public abstract class SkillDefinition : ScriptableObject
{
    [Header("Genel")]
    [SerializeField] private string skillName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private int cost = 1;
    [SerializeField] private Sprite icon;

    [Header("Skill Tree")]
    [Tooltip("Bu skill açılmadan önce açılmış olması gereken skill(ler). Boşsa root skill'dir.")]
    [SerializeField] private List<SkillDefinition> prerequisites = new();

    public IReadOnlyList<SkillDefinition> Prerequisites => prerequisites; // For logic only

    public string SkillName => skillName;
    public string Description => description;
    public int Cost => cost;
    public Sprite Icon => icon;

    /// <summary>Skill unlock edildiğinde bir kere çağrılır; ilgili sisteme efekti uygular.</summary>
    public abstract void ApplySkill();
}