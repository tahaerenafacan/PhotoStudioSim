using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SkillConnectionUI : MonoBehaviour
{
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.green;

    private UnityEngine.UI.Image lineImage;

    private void Awake()
    {
        lineImage = GetComponent<UnityEngine.UI.Image>();
    }

    public void RefreshColor(bool unlocked)
    {
        lineImage.color = unlocked ? unlockedColor : lockedColor;
    }
}