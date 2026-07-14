using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Skill tree'yi SkillManager.AvailableSkills verisinden kurar.
/// Node pozisyonları ve prerequisite bağları SkillDefinition asset'lerinde tanımlıdır (SSOT).
/// </summary>
public class SkillUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillPointsText; [

    Header("Selected Skill Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI selectedNameText;
    [SerializeField] private TextMeshProUGUI selectedDescriptionText;
    [SerializeField] private TextMeshProUGUI selectedCostText;
    [SerializeField] private Button buyButton;

    private SkillDefinition selectedSkill;

    private void Awake()
    {
        if (detailPanel) detailPanel.SetActive(false);
        if (buyButton) buyButton.onClick.AddListener(HandleBuyClicked);
    }

    private void OnEnable()
    {
        if (SkillManager.Instance != null)
        {
            HandleSkillPointsChanged(SkillManager.Instance.AvailableSkillPoints);
            SkillManager.Instance.OnSkillPointsChanged += HandleSkillPointsChanged;
            SkillManager.Instance.OnSkillUnlocked += HandleSkillUnlocked;
        }
        else Debug.LogError("SkillManager instance not found!");
    }

    private void OnDestroy()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillPointsChanged -= HandleSkillPointsChanged;
            SkillManager.Instance.OnSkillUnlocked -= HandleSkillUnlocked;
        }
        if (buyButton) buyButton.onClick.RemoveListener(HandleBuyClicked);
    }

    private void HandleSkillPointsChanged(int skillPoints)
    {
        skillPointsText.text = $"Skill Points: {skillPoints}";
        RefreshBuyButtonState();
    }

    // Bir skill herhangi bir yerden (başka node'dan bile) unlock edilirse,
    // panelde gösterilen seçili skill'in buy button durumu da değişmiş olabilir.
    private void HandleSkillUnlocked(SkillDefinition unlockedSkill) => RefreshBuyButtonState();

    /// <summary>SkillNodeUI tıklandığında çağrılır; detay panelini doldurur.</summary>
    public void SelectSkill(SkillDefinition skill)
    {
        selectedSkill = skill;

        if (detailPanel) detailPanel.SetActive(skill != null);
        if (skill == null) return;

        selectedNameText.text = skill.SkillName;
        selectedDescriptionText.text = skill.Description;
        selectedCostText.text = $"Cost: {skill.Cost}";

        RefreshBuyButtonState();
    }

    private void HandleBuyClicked()
    {
        if (selectedSkill == null) return;

        SkillManager.Instance.TryUnlockSkill(selectedSkill);
        RefreshBuyButtonState();
    }

    private void RefreshBuyButtonState()
    {
        if (buyButton == null || selectedSkill == null) return;

        // Zaten açıksa veya prerequisite/puan yetersizse Buy butonu pasif.
        buyButton.interactable = SkillManager.Instance.CanUnlock(selectedSkill);
    }
}
