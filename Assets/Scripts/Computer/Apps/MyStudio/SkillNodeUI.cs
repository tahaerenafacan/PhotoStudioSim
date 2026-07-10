using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Skill tree'de tek bir skill'i temsil eden buton. Tıklanınca unlock denemesi yapar.
/// Renk durumu: kilitli (gri), açılabilir (sarı/beyaz), açık (yeşil).
/// </summary>
[RequireComponent(typeof(Button))]
public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private SkillUIController skillUIController;
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private SkillConnectionUI[] connections;

    [Header("Colors")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockableColor = Color.white;
    [SerializeField] private Color unlockedColor = Color.green;

    [SerializeField] private SkillDefinition skill;
    private Button button;

    public void Awake()
    {
        button = GetComponent<Button>();

        if (iconImage && skill.Icon) iconImage.sprite = skill.Icon;

        button.onClick.AddListener(HandleClick);

        SkillManager.Instance.OnSkillUnlocked += HandleAnySkillUnlocked;
        RefreshVisual();
    }

    private void OnDestroy()
    {
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnSkillUnlocked -= HandleAnySkillUnlocked;
    }

    private void HandleClick()
    {
        skillUIController.SelectSkill(skill);
    }

    // Herhangi bir skill açıldığında çağrılır çünkü prerequisite durumu değişmiş olabilir.
    private void HandleAnySkillUnlocked(SkillDefinition unlockedSkill) => RefreshVisual();

    public void RefreshVisual()
    {
        bool isUnlocked = SkillManager.Instance.IsUnlocked(skill);
        bool canUnlock = SkillManager.Instance.CanUnlock(skill);

        background.color = isUnlocked ? unlockedColor : canUnlock ? unlockableColor: lockedColor;
        foreach (var connection in connections)
        {
            if (connection != null)
            {
                connection.RefreshColor(isUnlocked);
            }
        }
    }
}